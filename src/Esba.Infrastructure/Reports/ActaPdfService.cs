using Esba.Application.Abstractions;
using Esba.Application.DTOs.Examenes;
using Esba.Domain.Certificados;
using Esba.Domain.Examenes;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Esba.Infrastructure.Reports;

/// <summary>
/// Reporte QuestPDF de las actas de examen (sucesor del dibujo GDI sobre Gnostice de
/// lstactas*.pas). Hoja Oficio/Legal con la grilla de alumnos y columnas en blanco
/// para que el tribunal asiente las calificaciones a mano, como el papel volante
/// legacy. Una comisión/mesa por página.
/// </summary>
public sealed class ActaPdfService : IActaReportService
{
    private const string ColorPrimario = ReporteConstanciaLayout.ColorPrimario;

    static ActaPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerarActaComision(ActaComisionModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.Legal);
                pagina.Margin(2f, Unit.Centimetre);
                pagina.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));
                Pie(pagina, model.CuatrimestreAnio);

                pagina.Content().Column(col =>
                {
                    for (var i = 0; i < model.Secciones.Count; i++)
                    {
                        var seccion = model.Secciones[i];
                        col.Item().Column(bloque =>
                        {
                            bloque.Spacing(4);
                            EncabezadoComun(bloque, model.CarreraLarga, model.Titulo, seccion.Cabecera.DescripcionMateria);

                            if (CodigoComision.TryDescomponer(seccion.Cabecera.Cutuco, out var codigo))
                            {
                                bloque.Item().AlignCenter().Text(
                                    $"Cuat.: {codigo.Cuatrimestre}    Turno: {codigo.TurnoTexto}    Comisión: {codigo.ComisionTexto}");

                                if (model.MuestraCorrespondienteCuatrimestre)
                                {
                                    bloque.Item().AlignCenter().Text(
                                        $"Correspondiente al {codigo.Cuatrimestre}° CUATRIMESTRE de estudios").FontSize(9);
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(seccion.Cabecera.Docente))
                            {
                                bloque.Item().Text(model.Tipo == TipoActaComision.Examenes
                                    ? $"Con asistencia del Sr/a. Profesor/a: {seccion.Cabecera.Docente} se procedió a cumplir con el resultado que se consigna a continuación."
                                    : $"Docente/s: {seccion.Cabecera.Docente}").FontSize(9);
                            }

                            bloque.Item().PaddingTop(8).Element(c => TablaAlumnos(c, seccion.Alumnos, conPermiso: false));
                        });

                        if (i < model.Secciones.Count - 1)
                        {
                            col.Item().PageBreak();
                        }
                    }
                });
            });
        });

        return documento.GeneratePdf();
    }

    public byte[] GenerarActaMesa(ActaMesaModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.Legal);
                pagina.Margin(2f, Unit.Centimetre);
                pagina.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));
                Pie(pagina, $"Mesa {model.Mesa}");

                pagina.Content().Column(col =>
                {
                    col.Spacing(4);
                    EncabezadoComun(col, model.CarreraLarga, model.Titulo, model.Cabecera.DescripcionMateria);

                    var lineaComision = model.Cabecera.Cutuco is int cutuco
                        && CodigoComision.TryDescomponer(cutuco, out var codigo)
                        ? $"Cuat.: {codigo.Cuatrimestre}    Turno: {codigo.TurnoTexto}    Comisión: {codigo.ComisionTexto}    Mesa: {model.Mesa}"
                        : $"Mesa: {model.Mesa}"
                          + (model.Cabecera.CuatrimestreMateria is int cm ? $"    Cuat.: {cm}" : string.Empty);
                    col.Item().AlignCenter().Text(lineaComision);

                    var fecha = $"A los {model.Cabecera.Dia} días del mes de "
                        + $"{TextoCastellano.MesEnLetras(model.Cabecera.Mes)} de {model.Cabecera.Anio}, reunida la "
                        + "Comisión Examinadora de la asignatura mencionada"
                        + (string.IsNullOrWhiteSpace(model.Cabecera.Docente)
                            ? "."
                            : $", con asistencia de sus miembros: {model.Cabecera.Docente}.");
                    col.Item().PaddingTop(4).Text(fecha).FontSize(9);

                    col.Item().PaddingTop(8).Element(c => TablaAlumnos(c, model.Alumnos, conPermiso: true));
                });
            });
        });

        return documento.GeneratePdf();
    }

    private static void EncabezadoComun(ColumnDescriptor col, string carreraLarga, string titulo, string? asignatura)
    {
        col.Item().AlignCenter().Text(carreraLarga).Bold().FontSize(11);
        col.Item().AlignCenter().Text(titulo).Bold().FontColor(ColorPrimario).FontSize(12);
        col.Item().AlignCenter().Text($"Asignatura: {asignatura}").SemiBold();
    }

    private static void TablaAlumnos(IContainer contenedor, IReadOnlyList<ActaAlumnoDto> alumnos, bool conPermiso)
    {
        contenedor.Table(tabla =>
        {
            tabla.ColumnsDefinition(cols =>
            {
                if (conPermiso)
                {
                    cols.ConstantColumn(55);   // Permiso
                }

                cols.ConstantColumn(70);       // Código
                cols.RelativeColumn();         // Apellido y nombre
                cols.ConstantColumn(70);       // Calificación (en blanco)
                cols.ConstantColumn(120);      // Calificación en letras (en blanco)
            });

            tabla.Header(encabezado =>
            {
                if (conPermiso)
                {
                    Celda(encabezado.Cell(), "Permiso", header: true);
                }

                Celda(encabezado.Cell(), "Código", header: true);
                Celda(encabezado.Cell(), "Apellido y nombre", header: true);
                Celda(encabezado.Cell(), "Calificación", header: true);
                Celda(encabezado.Cell(), "En letras", header: true);
            });

            foreach (var alumno in alumnos)
            {
                if (conPermiso)
                {
                    Celda(tabla.Cell(), alumno.PermisoExamen?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty);
                }

                Celda(tabla.Cell(), alumno.CodigoAlumno);
                Celda(tabla.Cell(), $"{alumno.Apellido}, {alumno.Nombre}");
                Celda(tabla.Cell(), string.Empty);   // a completar a mano
                Celda(tabla.Cell(), string.Empty);   // a completar a mano
            }
        });
    }

    private static void Celda(IContainer celda, string texto, bool header = false)
    {
        var contenido = celda.Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(4);
        if (header)
        {
            contenido.Text(texto).SemiBold().FontSize(9);
        }
        else
        {
            contenido.MinHeight(16).Text(texto).FontSize(9);
        }
    }

    private static void Pie(PageDescriptor pagina, string referencia)
    {
        pagina.Footer().AlignRight().Text(text =>
        {
            text.Span($"{referencia}  —  Página ").FontSize(8).FontColor(Colors.Grey.Medium);
            text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
            text.Span(" de ").FontSize(8).FontColor(Colors.Grey.Medium);
            text.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
        });
    }
}
