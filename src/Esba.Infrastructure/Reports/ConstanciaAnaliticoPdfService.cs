using Esba.Application.Abstractions;
using Esba.Application.DTOs.Certificados;
using Esba.Domain.Certificados;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Esba.Infrastructure.Reports;

/// <summary>
/// Reporte QuestPDF de la "Constancia de Materias Aprobadas" (analítico tabular),
/// sucesor de <c>BitBtn1Click</c> de constanciaalumnos2.pas: tabla de materias
/// agrupada por cuatrimestre. Solo maqueta: las filas ya vienen formateadas en
/// <see cref="ConstanciaMateriasAprobadasModel"/> (§2.1). Comparte membrete y firmas
/// con la constancia de texto vía <see cref="ReporteConstanciaLayout"/>.
/// </summary>
public sealed class ConstanciaAnaliticoPdfService : IConstanciaAnaliticoReportService
{
    private const string Titulo = "CONSTANCIA DE MATERIAS APROBADAS";
    private const string ColorPrimario = ReporteConstanciaLayout.ColorPrimario;

    private readonly InstitucionSettings _institucion;

    static ConstanciaAnaliticoPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public ConstanciaAnaliticoPdfService(IOptions<InstitucionSettings> institucion)
    {
        _institucion = institucion.Value;
    }

    public byte[] GenerarMateriasAprobadas(ConstanciaMateriasAprobadasModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var logo = ReporteConstanciaLayout.CargarLogo(_institucion);

        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(2.5f, Unit.Centimetre);
                pagina.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                if (model.IncluirMembrete)
                {
                    pagina.Header().Element(c =>
                        ReporteConstanciaLayout.Membrete(c, _institucion, model.Instituto, model.Caracteristica, logo));
                }

                pagina.Content().PaddingTop(model.IncluirMembrete ? 12 : 0).Column(col =>
                {
                    col.Spacing(8);

                    col.Item().AlignCenter().Text(Titulo).FontSize(13).Bold().FontColor(ColorPrimario);
                    col.Item().PaddingTop(4).Text(model.Introduccion).Justify();
                    col.Item().PaddingTop(4).Element(c => Tabla(c, model.Filas));
                    col.Item().PaddingTop(8).Text($"Para ser presentada ante: {model.AnteQuien}");
                    col.Item().PaddingTop(36).Element(c =>
                        ReporteConstanciaLayout.Firmas(c, model.Secretaria, model.Rector));
                });
            });
        });

        return documento.GeneratePdf();
    }

    private static void Tabla(IContainer contenedor, IReadOnlyList<FilaAnaliticoConstancia> filas)
    {
        contenedor.Border(1).BorderColor(Colors.Grey.Darken1).Table(tabla =>
        {
            tabla.ColumnsDefinition(columnas =>
            {
                columnas.RelativeColumn(3.2f);  // Materia
                columnas.RelativeColumn(2.4f);  // Condición
                columnas.RelativeColumn(0.9f);  // Nota
                columnas.RelativeColumn(1.3f);  // Fecha
                columnas.RelativeColumn(2.6f);  // Instituto / Característica
            });

            tabla.Header(encabezado =>
            {
                Encabezado(encabezado, "Materia");
                Encabezado(encabezado, "Condición");
                Encabezado(encabezado, "Nota");
                Encabezado(encabezado, "Fecha");
                Encabezado(encabezado, "Instituto / Característica");
            });

            var cuatrimestreActual = int.MinValue;
            foreach (var fila in filas)
            {
                if (fila.Cuatrimestre != cuatrimestreActual)
                {
                    cuatrimestreActual = fila.Cuatrimestre;
                    tabla.Cell().ColumnSpan(5).Background(Colors.Grey.Lighten3).Padding(3)
                        .Text($"{TextoCastellano.CuatrimestreEnLetras(cuatrimestreActual)} Cuatrimestre/Año").Bold();
                }

                Estilo(tabla.Cell()).Text(fila.Materia);
                if (fila.OcupaFilaCompleta)
                {
                    Estilo(tabla.Cell().ColumnSpan(4)).Text(fila.Condicion);
                }
                else
                {
                    Estilo(tabla.Cell()).Text(fila.Condicion);
                    Estilo(tabla.Cell()).Text(fila.Nota);
                    Estilo(tabla.Cell()).Text(fila.Fecha);
                    Estilo(tabla.Cell()).Text(fila.Instituto);
                }
            }
        });
    }

    private static void Encabezado(TableCellDescriptor encabezado, string texto) =>
        encabezado.Cell().Background(ColorPrimario).Padding(3).Text(texto).Bold().FontColor(Colors.White);

    private static IContainer Estilo(IContainer celda) =>
        celda.BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(3);
}
