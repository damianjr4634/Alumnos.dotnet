using System.Globalization;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Examenes;
using Esba.Domain.Examenes;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Esba.Infrastructure.Reports;

/// <summary>
/// Reporte QuestPDF del parte diario de mesas (sucesor del dibujo GDI de
/// Imp_Mesas_ParteDiario en Impresiones.pas): A4, una hoja por fecha de examen con el
/// título, el curso lectivo y la tabla de mesas (mesa, hora, materia, profesores,
/// comisión, cantidad de alumnos, aula) cortada por carrera con una fila de subtítulo.
/// </summary>
public sealed class ParteDiarioMesasPdfService : IParteDiarioMesasReportService
{
    private const string ColorPrimario = ReporteConstanciaLayout.ColorPrimario;
    private const uint TotalColumnas = 7;

    static ParteDiarioMesasPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerarParteDiario(ParteDiarioMesasModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(1.2f, Unit.Centimetre);
                pagina.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                pagina.Content().Column(col =>
                {
                    for (var i = 0; i < model.Dias.Count; i++)
                    {
                        var dia = model.Dias[i];
                        col.Item().Column(hoja =>
                        {
                            hoja.Spacing(3);
                            hoja.Item().AlignCenter().Text("MESAS DE EXAMENES FINALES - PARTE DIARIO").Bold().FontSize(12).FontColor(ColorPrimario);
                            hoja.Item().AlignCenter().Text($"CURSO LECTIVO: {model.CicloLectivo.ToString(CultureInfo.InvariantCulture)}").Bold().FontSize(11);
                            hoja.Item().Text($"Fecha de examen: {dia.Fecha?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}").Bold().FontSize(10);
                            hoja.Item().PaddingTop(6).Element(c => Tabla(c, dia));
                        });

                        if (i < model.Dias.Count - 1)
                        {
                            col.Item().PageBreak();
                        }
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

    private static void Tabla(IContainer contenedor, ParteDiarioDia dia)
    {
        contenedor.Table(tabla =>
        {
            tabla.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(34);   // Mesa
                cols.ConstantColumn(36);   // Hora
                cols.RelativeColumn(3);    // Materia
                cols.RelativeColumn(5);    // Profesores
                cols.ConstantColumn(60);   // Comisión
                cols.ConstantColumn(52);   // Cant. alum.
                cols.ConstantColumn(34);   // Aula
            });

            tabla.Header(encabezado =>
            {
                Celda(encabezado.Cell(), "MESA", header: true);
                Celda(encabezado.Cell(), "HORA", header: true);
                Celda(encabezado.Cell(), "MATERIA", header: true, alinearIzquierda: true);
                Celda(encabezado.Cell(), "PROFESORES", header: true, alinearIzquierda: true);
                Celda(encabezado.Cell(), "COMISION", header: true);
                Celda(encabezado.Cell(), "CANT.ALUM", header: true);
                Celda(encabezado.Cell(), "AULA", header: true);
            });

            foreach (var carrera in dia.Carreras)
            {
                var subtitulo = tabla.Cell().ColumnSpan(TotalColumnas)
                    .BorderTop(1f).BorderBottom(1f).BorderColor(Colors.Black).PaddingVertical(3).PaddingHorizontal(3);
                subtitulo.Text(string.IsNullOrWhiteSpace(carrera.NombreCarrera)
                        ? carrera.CodigoCarrera
                        : $"{carrera.CodigoCarrera} — {carrera.NombreCarrera}")
                    .Bold().FontSize(10);

                foreach (var mesa in carrera.Mesas)
                {
                    Celda(tabla.Cell(), mesa.Mesa.ToString(CultureInfo.InvariantCulture));
                    Celda(tabla.Cell(), FormatoMesa.Hora(mesa.Hora));
                    Celda(tabla.Cell(), mesa.Materia ?? string.Empty, alinearIzquierda: true);
                    Celda(tabla.Cell(), FormatoMesa.Docentes(mesa.Titular, mesa.Vocal1, mesa.Vocal2), alinearIzquierda: true);
                    Celda(tabla.Cell(), FormatoMesa.Comisiones(mesa.Comision1, mesa.Comision2, mesa.Comision3));
                    Celda(tabla.Cell(), mesa.CantidadAlumnos.ToString(CultureInfo.InvariantCulture));
                    Celda(tabla.Cell(), mesa.Aula?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
                }
            }
        });
    }

    private static void Celda(IContainer celda, string texto, bool header = false, bool alinearIzquierda = false)
    {
        var contenido = celda.BorderBottom(0.5f).BorderColor(Colors.Grey.Medium).PaddingVertical(2).PaddingHorizontal(3);
        if (!alinearIzquierda)
        {
            contenido = contenido.AlignCenter();
        }

        if (header)
        {
            contenido.Text(texto).Bold().FontSize(9);
        }
        else
        {
            contenido.Text(texto);
        }
    }
}
