using System.Globalization;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Examenes;
using Esba.Domain.Examenes;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Esba.Infrastructure.Reports;

/// <summary>
/// Reporte QuestPDF de la citación a profesores (sucesor del dibujo GDI sobre Gnostice
/// de Imp_Mesas_citacion en Impresiones.pas): A4 sobre el membrete con dirección, una
/// carta por docente en dos copias (ORIGINAL y DUPLICADO en hojas separadas) con la
/// tabla de mesas a integrar, el espacio de notificación y la firma de la autoridad
/// elegida (imagen opcional configurada por firmante). El texto reglamentario del pie
/// es el del legacy con sus erratas corregidas.
/// </summary>
public sealed class CitacionDocentesPdfService : ICitacionDocentesReportService
{
    private const string ColorPrimario = ReporteConstanciaLayout.ColorPrimario;
    private const float MargenSuperiorCm = 4f;
    private const float MargenInferiorCm = 1.5f;
    private const float MargenHorizontalCm = 1.5f;

    private readonly InstitucionSettings _institucion;

    static CitacionDocentesPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public CitacionDocentesPdfService(IOptions<InstitucionSettings> institucion)
    {
        _institucion = institucion.Value;
    }

    public byte[] GenerarCitacion(CitacionDocentesModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        // El legacy siempre imprimía sobre membrete_con_direccion.jpg.
        var membrete = ReporteConstanciaLayout.CargarFondo(_institucion.MembreteConstanciaPath);
        var imagenes = ReporteConstanciaLayout.ImagenesAutoridades.Cargar(_institucion);
        var firma = model.Firmante switch
        {
            FirmanteCitacion.Rector => imagenes.FirmaRector,
            FirmanteCitacion.Secretaria => imagenes.FirmaSecretaria,
            _ => imagenes.FirmaDirectorEstudios,
        };

        var copias = model.Docentes
            .SelectMany(d => new[] { (Docente: d, Copia: CitacionDocentesTextos.Original), (Docente: d, Copia: CitacionDocentesTextos.Duplicado) })
            .ToList();

        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.MarginTop(MargenSuperiorCm, Unit.Centimetre);
                pagina.MarginBottom(MargenInferiorCm, Unit.Centimetre);
                pagina.MarginHorizontal(MargenHorizontalCm, Unit.Centimetre);
                pagina.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                if (membrete is not null)
                {
                    pagina.Background().Image(membrete).FitArea();
                }

                pagina.Content().Column(col =>
                {
                    for (var i = 0; i < copias.Count; i++)
                    {
                        var (docente, copia) = copias[i];
                        col.Item().Column(carta =>
                        {
                            carta.Spacing(4);
                            Encabezado(carta, model, docente, copia);
                            carta.Item().PaddingTop(4).Element(c => TablaMesas(c, docente.Mesas));
                            Pie(carta, model, firma, imagenes);
                        });

                        if (i < copias.Count - 1)
                        {
                            col.Item().PageBreak();
                        }
                    }
                });
            });
        });

        return documento.GeneratePdf();
    }

    private void Encabezado(ColumnDescriptor col, CitacionDocentesModel model, CitacionDocenteSeccion docente, string copia)
    {
        col.Item().Text(copia).Bold().FontSize(12);
        col.Item().AlignCenter().Text(CitacionDocentesTextos.Titulo).Bold().FontSize(12).FontColor(ColorPrimario);
        col.Item().AlignCenter().Text(
            $"{_institucion.Nombre.ToUpperInvariant()} {_institucion.Caracteristica}   EMISION: {model.FechaEmision:dd/MM/yyyy}")
            .Bold().FontSize(11);
        col.Item().PaddingTop(6).Text($"Señor Profesor: {docente.Docente}").Bold();
        col.Item().Text(CitacionDocentesTextos.Comunicacion);
    }

    private static void TablaMesas(IContainer contenedor, IReadOnlyList<MesaCitacionDto> mesas)
    {
        contenedor.Table(tabla =>
        {
            tabla.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(62);   // Fecha
                cols.ConstantColumn(40);   // Hora
                cols.RelativeColumn();     // Asignatura
                cols.ConstantColumn(44);   // Mesa
                cols.ConstantColumn(40);   // Aula
                cols.ConstantColumn(60);   // Carrera
            });

            tabla.Header(encabezado =>
            {
                Celda(encabezado.Cell(), "FECHA", header: true);
                Celda(encabezado.Cell(), "HORA", header: true);
                Celda(encabezado.Cell(), "ASIGNATURA", header: true);
                Celda(encabezado.Cell(), "MESA", header: true);
                Celda(encabezado.Cell(), "AULA", header: true);
                Celda(encabezado.Cell(), "CARRERA", header: true);
            });

            foreach (var mesa in mesas)
            {
                Celda(tabla.Cell(), mesa.FechaExamen?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? string.Empty);
                Celda(tabla.Cell(), FormatoMesa.Hora(mesa.Hora));
                Celda(tabla.Cell(), mesa.Materia ?? string.Empty);
                Celda(tabla.Cell(), mesa.Mesa.ToString(CultureInfo.InvariantCulture));
                Celda(tabla.Cell(), mesa.Aula?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
                Celda(tabla.Cell(), mesa.CodigoCarrera);
            }
        });
    }

    private static void Pie(
        ColumnDescriptor col, CitacionDocentesModel model, byte[]? firma, ReporteConstanciaLayout.ImagenesAutoridades imagenes)
    {
        col.Item().PaddingTop(16).Text(CitacionDocentesTextos.Notificado);
        col.Item().PaddingTop(6).Text(CitacionDocentesTextos.FechaEnBlanco);
        col.Item().PaddingTop(10).Text(CitacionDocentesTextos.Saludo);

        // Firma a la derecha, como el legacy (x = 10..15 cm), con la imagen de la firma
        // del firmante sobre su nombre y el sello a su izquierda.
        col.Item().PaddingTop(4).Element(c =>
            ReporteConstanciaLayout.FirmaUnica(c, model.NombreFirmante, model.Firmante.Cargo(), firma, imagenes));

        col.Item().PaddingTop(10).Text(CitacionDocentesTextos.Reglamento).FontSize(9).Justify();
    }

    private static void Celda(IContainer celda, string texto, bool header = false)
    {
        var contenido = celda.BorderBottom(0.5f).BorderColor(Colors.Grey.Medium).PaddingVertical(3).PaddingHorizontal(3);
        if (header)
        {
            contenido.BorderTop(1f).BorderBottom(1f).BorderColor(Colors.Black).Text(texto).Bold();
        }
        else
        {
            contenido.Text(texto);
        }
    }
}
