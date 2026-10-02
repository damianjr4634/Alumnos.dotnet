using System.Globalization;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Ministerio;
using Esba.Domain.Examenes;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Esba.Infrastructure.Reports;

/// <summary>
/// Reporte QuestPDF de la nómina de alumnos por comisión para el Ministerio (sucesor
/// del dibujo GDI sobre Gnostice del ImprimirClick de ComisionesAlMinisterio.pas): A4,
/// una hoja por comisión con encabezado (carrera, curso lectivo, cuatrimestre/turno/
/// división, "NOMINA DE ALUMNOS", "INSCRIPTOS AL"), la nómina numerada y, debajo, el
/// bloque RECURSANTES numerado aparte. Opciones del legacy: papel membretado de fondo
/// (el mismo JPG "membrete_con_direccion" de las constancias), columnas de
/// nacionalidad y edad (con letra un punto menor, como el original) y margen superior
/// ajustable para papel preimpreso.
/// </summary>
public sealed class NominaMinisterioPdfService : INominaMinisterioReportService
{
    private const string ColorPrimario = ReporteConstanciaLayout.ColorPrimario;
    private const float MargenHorizontalCm = 1.5f;
    private const float MargenInferiorCm = 1.5f;

    private readonly InstitucionSettings _institucion;

    static NominaMinisterioPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public NominaMinisterioPdfService(IOptions<InstitucionSettings> institucion)
    {
        _institucion = institucion.Value;
    }

    public byte[] GenerarNomina(NominaMinisterioModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var membrete = model.ConMembrete
            ? ReporteConstanciaLayout.CargarFondo(_institucion.MembreteConstanciaPath)
            : null;
        var imagenes = membrete is not null
            ? ReporteConstanciaLayout.ImagenesAutoridades.Cargar(_institucion)
            : ReporteConstanciaLayout.ImagenesAutoridades.Ninguna;

        // El legacy achicaba un punto la letra cuando entraban las columnas extra.
        var tamanoEncabezado = model.ConEdadYNacionalidad ? 9 : 10;
        var tamanoDetalle = model.ConEdadYNacionalidad ? 8 : 9;

        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.MarginTop((float)model.MargenSuperiorCm, Unit.Centimetre);
                pagina.MarginBottom(MargenInferiorCm, Unit.Centimetre);
                pagina.MarginHorizontal(MargenHorizontalCm, Unit.Centimetre);
                pagina.DefaultTextStyle(x => x.FontSize(tamanoDetalle).FontFamily("Arial"));

                if (membrete is not null)
                {
                    pagina.Background().Image(membrete).FitArea();
                }

                pagina.Content().Column(col =>
                {
                    for (var i = 0; i < model.Secciones.Count; i++)
                    {
                        var seccion = model.Secciones[i];
                        col.Item().Column(bloque =>
                        {
                            bloque.Spacing(2);
                            Encabezado(bloque, model, seccion.Cutuco, tamanoEncabezado);
                            bloque.Item().PaddingTop(8).Element(c => Tabla(c, model, seccion, tamanoEncabezado, tamanoDetalle));
                        });

                        if (i < model.Secciones.Count - 1)
                        {
                            col.Item().PageBreak();
                        }
                    }

                    // Con membrete, el sello cierra la última hoja (la nómina no lleva firmas).
                    if (membrete is not null)
                    {
                        col.Item().PaddingTop(16).Element(c => ReporteConstanciaLayout.SoloSello(c, imagenes));
                    }
                });

                pagina.Footer().AlignRight().Text(text =>
                {
                    text.Span("Página ").FontSize(8).FontColor(Colors.Grey.Medium);
                    text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                    text.Span(" de ").FontSize(8).FontColor(Colors.Grey.Medium);
                    text.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        });

        return documento.GeneratePdf();
    }

    private static void Encabezado(ColumnDescriptor col, NominaMinisterioModel model, short cutuco, int tamano)
    {
        col.Item().Text($"Carrera: {model.CarreraLarga}").Bold().FontSize(tamano);
        col.Item().Text($"Curso Lectivo: {model.CicloLectivo.ToString(CultureInfo.InvariantCulture)}").Bold().FontSize(tamano);
        col.Item().Text(DescripcionComision(cutuco)).Bold().FontSize(tamano);
        col.Item().PaddingTop(4).AlignCenter().Text("NOMINA DE ALUMNOS").Bold().FontSize(tamano).FontColor(ColorPrimario);
        col.Item().AlignCenter().Text($"INSCRIPTOS AL: {model.FechaInscriptosAl:dd/MM/yyyy}").Bold().FontSize(tamano);
    }

    /// <summary>"Cuat.: 1    Turno: Mañana    División: A" como el legacy (Turnos/Division de FuncionesText).</summary>
    private static string DescripcionComision(short cutuco) =>
        CodigoComision.TryDescomponer(cutuco, out var codigo)
            ? $"Cuat.: {codigo.Cuatrimestre.ToString(CultureInfo.InvariantCulture)}    Turno: {codigo.TurnoTexto}     División: {codigo.ComisionTexto}"
            : $"Comisión: {cutuco.ToString(CultureInfo.InvariantCulture)}";

    private static void Tabla(
        IContainer contenedor, NominaMinisterioModel model, NominaMinisterioSeccion seccion, int tamanoEncabezado, int tamanoDetalle)
    {
        var conExtras = model.ConEdadYNacionalidad;
        var totalColumnas = conExtras ? 5u : 3u;

        contenedor.Table(tabla =>
        {
            tabla.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(28);       // N°
                cols.RelativeColumn(4);        // Apellido y nombre
                cols.RelativeColumn(2);        // Documento
                if (conExtras)
                {
                    cols.RelativeColumn(2);    // Nacionalidad
                    cols.ConstantColumn(40);   // Edad
                }
            });

            tabla.Header(encabezado =>
            {
                Celda(encabezado.Cell(), "N°", tamanoEncabezado, header: true);
                Celda(encabezado.Cell(), "APELLIDO Y NOMBRE", tamanoEncabezado, header: true, alinearIzquierda: true);
                Celda(encabezado.Cell(), "DOCUMENTO", tamanoEncabezado, header: true, alinearIzquierda: true);
                if (conExtras)
                {
                    Celda(encabezado.Cell(), "NACIONALIDAD", tamanoEncabezado, header: true, alinearIzquierda: true);
                    Celda(encabezado.Cell(), "EDAD", tamanoEncabezado, header: true);
                }
            });

            Filas(tabla, seccion.Cursando, conExtras, tamanoDetalle);

            if (seccion.Recursantes.Count > 0)
            {
                var subtitulo = tabla.Cell().ColumnSpan(totalColumnas)
                    .BorderBottom(0.5f).BorderColor(Colors.Grey.Medium).PaddingVertical(4).AlignCenter();
                subtitulo.Text("RECURSANTES").Bold().FontSize(tamanoEncabezado);
                Filas(tabla, seccion.Recursantes, conExtras, tamanoDetalle);
            }
        });
    }

    private static void Filas(TableDescriptor tabla, IReadOnlyList<NominaMinisterioAlumnoDto> alumnos, bool conExtras, int tamano)
    {
        for (var i = 0; i < alumnos.Count; i++)
        {
            var alumno = alumnos[i];
            Celda(tabla.Cell(), (i + 1).ToString(CultureInfo.InvariantCulture), tamano);
            Celda(tabla.Cell(), $"{alumno.Apellido}, {alumno.Nombre}", tamano, alinearIzquierda: true);
            Celda(tabla.Cell(), alumno.Documento, tamano, alinearIzquierda: true);
            if (conExtras)
            {
                Celda(tabla.Cell(), alumno.Nacionalidad ?? string.Empty, tamano, alinearIzquierda: true);
                Celda(tabla.Cell(), alumno.Edad?.ToString(CultureInfo.InvariantCulture) ?? string.Empty, tamano);
            }
        }
    }

    /// <summary>Renglón con línea inferior, como las líneas que el legacy trazaba bajo cada alumno.</summary>
    private static void Celda(IContainer celda, string texto, int tamano, bool header = false, bool alinearIzquierda = false)
    {
        var contenido = celda.BorderBottom(0.5f).BorderColor(Colors.Grey.Medium).PaddingVertical(2).PaddingHorizontal(3);
        if (!alinearIzquierda)
        {
            contenido = contenido.AlignCenter();
        }

        if (header)
        {
            contenido.Text(texto).Bold().FontSize(tamano);
        }
        else
        {
            contenido.Text(texto).FontSize(tamano);
        }
    }
}
