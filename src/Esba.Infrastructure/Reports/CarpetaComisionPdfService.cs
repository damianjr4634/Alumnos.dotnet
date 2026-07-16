using Esba.Application.Abstractions;
using Esba.Application.DTOs.Asistencias;
using Esba.Domain.Asistencias;
using Esba.Domain.Examenes;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Esba.Infrastructure.Reports;

/// <summary>
/// Reporte QuestPDF de las carpetas por comisión, una hoja por comisión con la
/// nómina (cursantes + recursantes al pie, numerados aparte) y la grilla en blanco
/// que el docente completa a mano:
/// <list type="bullet">
/// <item><b>Asistencia</b> (sucesor del dibujo GDI de lstplanasis.pas): A4 con 25
/// columnas de días más INA/ANT/TOT y la columna D/H partida en dos.</item>
/// <item><b>Trabajos prácticos</b> (sucesor de lstNotasyPractico.pas +
/// trabajos_practicos.wmf): Oficio con TP 1–5 (cada uno con línea de fecha) y
/// columna de condición.</item>
/// <item><b>Planilla de profesores</b> (el mismo lstNotasyPractico.pas con
/// Planilla_calificaciones.wmf): Oficio con 1er./2do bimestre (5 notas + Prom.
/// cada uno), calificación Final/Recup./Def. y columna Notificado.</item>
/// </list>
/// </summary>
public sealed class CarpetaComisionPdfService : ICarpetaComisionReportService
{
    private const int ColumnasDias = 25;
    private const int TotalColumnasAsistencia = ColumnasDias + 6; // N° + nombre + D/H + días + INA/ANT/TOT
    private const int CantidadTp = 5;
    private const int TotalColumnasTp = CantidadTp + 3;           // N° + nombre + TPs + condición
    private const int NotasPorBimestre = 5;
    // N° + nombre + 2 bimestres (5 notas + Prom.) + Final/Recup./Def. + Notificado
    private const int TotalColumnasCalificaciones = 2 + 2 * (NotasPorBimestre + 1) + 3 + 1;
    private const string ColorPrimario = ReporteConstanciaLayout.ColorPrimario;

    private readonly InstitucionSettings _institucion;

    static CarpetaComisionPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public CarpetaComisionPdfService(IOptions<InstitucionSettings> institucion)
    {
        _institucion = institucion.Value;
    }

    public byte[] GenerarCarpeta(CarpetaComisionModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var esAsistencia = model.Tipo == TipoCarpetaComision.Asistencia;

        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                // La hoja de asistencia legacy era A4; trabajos prácticos y planilla de
                // profesores usaban plantillas WMF estiradas a Oficio/Legal (el corte de
                // 31 cm de la planilla de calificaciones no entra en un A4).
                pagina.Size(esAsistencia ? PageSizes.A4 : PageSizes.Legal);
                pagina.Margin(1.2f, Unit.Centimetre);
                pagina.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));
                Pie(pagina, model.CuatrimestreAnio);

                pagina.Content().Column(col =>
                {
                    for (var i = 0; i < model.Secciones.Count; i++)
                    {
                        var seccion = model.Secciones[i];
                        col.Item().Column(bloque =>
                        {
                            bloque.Spacing(3);
                            Encabezado(bloque, model, seccion.Cabecera);
                            bloque.Item().PaddingTop(6).Element(c =>
                            {
                                switch (model.Tipo)
                                {
                                    case TipoCarpetaComision.Asistencia:
                                        TablaAsistencia(c, seccion);
                                        break;
                                    case TipoCarpetaComision.TrabajosPracticos:
                                        TablaTrabajosPracticos(c, seccion);
                                        break;
                                    default:
                                        TablaCalificaciones(c, seccion);
                                        break;
                                }
                            });
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

    private void Encabezado(ColumnDescriptor col, CarpetaComisionModel model, CarpetaComisionCabeceraDto cabecera)
    {
        var titulo = model.Tipo switch
        {
            TipoCarpetaComision.Asistencia => "CARPETA DE ASISTENCIA",
            TipoCarpetaComision.TrabajosPracticos => "CARPETA DE TRABAJOS PRÁCTICOS",
            _ => "PLANILLA DE CALIFICACIONES",
        };

        col.Item().AlignCenter().Text(
            $"{_institucion.Nombre} — Emisión: {model.FechaEmision:dd/MM/yyyy}").Bold().FontSize(11);
        col.Item().AlignCenter().Text(model.CarreraLarga).Bold();
        col.Item().AlignCenter().Text(titulo).Bold().FontColor(ColorPrimario).FontSize(12);
        col.Item().AlignCenter().Text($"Asignatura: {cabecera.DescripcionMateria}").SemiBold();

        if (CodigoComision.TryDescomponer(cabecera.Cutuco, out var codigo))
        {
            col.Item().AlignCenter().Text(
                $"Cuat.: {codigo.Cuatrimestre}    Turno: {codigo.TurnoTexto}    Comisión: {codigo.ComisionTexto}");
        }

        if (model.Tipo == TipoCarpetaComision.Asistencia)
        {
            if (!string.IsNullOrWhiteSpace(cabecera.Docente))
            {
                var caracter = string.Equals(cabecera.TitularSuplente?.Trim(), "T", StringComparison.OrdinalIgnoreCase)
                    ? "Tit."
                    : "Sup.";
                col.Item().Text($"Profesor/a: {cabecera.Docente} ({caracter})");
            }

            col.Item().PaddingTop(4).Text(
                $"Mes: ................................        Ciclo lectivo: {model.FechaEmision.Year}");
            col.Item().Text(
                "Horas dictadas del mes anterior: ....................        Total: ....................");
        }
        else
        {
            // El legacy de trabajos prácticos imprimía profesor + ciclo lectivo en una
            // sola línea, sin Tit./Sup. ni las líneas de mes/horas dictadas.
            col.Item().PaddingTop(4).Text(
                $"Profesor/a: {cabecera.Docente}        Ciclo lectivo: {model.FechaEmision.Year}");
        }
    }

    private static void TablaAsistencia(IContainer contenedor, CarpetaComisionSeccion seccion)
    {
        contenedor.Table(tabla =>
        {
            tabla.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(24);       // N°
                cols.RelativeColumn();         // Apellido y nombre
                cols.ConstantColumn(12);       // D/H (día arriba, hora abajo, como el legacy)
                for (var d = 0; d < ColumnasDias; d++)
                {
                    cols.ConstantColumn(13);   // un día, a completar a mano
                }

                cols.ConstantColumn(16);       // INA (inasistencias del mes)
                cols.ConstantColumn(16);       // ANT (acumuladas anteriores)
                cols.ConstantColumn(16);       // TOT
            });

            // Encabezado de dos medias filas, como la hoja legacy: N°, nombre, días y
            // los totales ocupan las dos; la columna angosta posterior al nombre lleva
            // la "D" arriba y la "H" abajo, y los totales las letras apiladas.
            tabla.Header(encabezado =>
            {
                Celda(encabezado.Cell().RowSpan(2), "N°", header: true);
                Celda(encabezado.Cell().RowSpan(2), "APELLIDO Y NOMBRE", header: true, alinearIzquierda: true);
                Celda(encabezado.Cell(), "D", header: true);
                for (var d = 0; d < ColumnasDias; d++)
                {
                    Celda(encabezado.Cell().RowSpan(2), string.Empty, header: true);
                }

                Celda(encabezado.Cell().RowSpan(2), "I\nN\nA", header: true);
                Celda(encabezado.Cell().RowSpan(2), "A\nN\nT", header: true);
                Celda(encabezado.Cell().RowSpan(2), "T\nO\nT", header: true);
                Celda(encabezado.Cell(), "H", header: true);
            });

            Filas(tabla, seccion.Cursando, celdasEnBlanco: ColumnasDias + 4, altoFila: 16);

            if (seccion.Recursantes.Count > 0)
            {
                Subtitulo(tabla, TotalColumnasAsistencia);
                Filas(tabla, seccion.Recursantes, celdasEnBlanco: ColumnasDias + 4, altoFila: 16);
            }
        });
    }

    private static void TablaTrabajosPracticos(IContainer contenedor, CarpetaComisionSeccion seccion)
    {
        contenedor.Table(tabla =>
        {
            tabla.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(24);       // N°
                cols.RelativeColumn();         // Apellido y nombre
                for (var t = 0; t < CantidadTp; t++)
                {
                    cols.ConstantColumn(52);   // TP con su nota, a completar a mano
                }

                cols.ConstantColumn(70);       // Condición
            });

            // Encabezado de dos medias filas, como la plantilla trabajos_practicos.wmf:
            // cada TP lleva arriba la línea de fecha "__/__/____" y abajo su rótulo.
            tabla.Header(encabezado =>
            {
                Celda(encabezado.Cell().RowSpan(2), "N°", header: true);
                Celda(encabezado.Cell().RowSpan(2), "APELLIDO Y NOMBRE", header: true, alinearIzquierda: true);
                for (var t = 0; t < CantidadTp; t++)
                {
                    Celda(encabezado.Cell(), "__/__/____", header: true);
                }

                Celda(encabezado.Cell().RowSpan(2), "CONDICIÓN", header: true);
                for (var t = 0; t < CantidadTp; t++)
                {
                    Celda(encabezado.Cell(), $"TP {t + 1}", header: true);
                }
            });

            Filas(tabla, seccion.Cursando, celdasEnBlanco: CantidadTp + 1, altoFila: 20);

            if (seccion.Recursantes.Count > 0)
            {
                Subtitulo(tabla, TotalColumnasTp);
                Filas(tabla, seccion.Recursantes, celdasEnBlanco: CantidadTp + 1, altoFila: 20);
            }
        });
    }

    private static void TablaCalificaciones(IContainer contenedor, CarpetaComisionSeccion seccion)
    {
        contenedor.Table(tabla =>
        {
            tabla.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(24);       // N°
                cols.RelativeColumn();         // Apellido y nombre
                for (var b = 0; b < 2; b++)    // 1er. y 2do bimestre
                {
                    for (var n = 0; n < NotasPorBimestre; n++)
                    {
                        cols.ConstantColumn(16);   // una nota, a completar a mano
                    }

                    cols.ConstantColumn(26);   // Prom.
                }

                cols.ConstantColumn(28);       // Final
                cols.ConstantColumn(30);       // Recup.
                cols.ConstantColumn(28);       // Def.
                cols.ConstantColumn(70);       // Notificado (firma)
            });

            // Encabezado de dos medias filas, como la plantilla Planilla_calificaciones.wmf:
            // cada bimestre agrupa sus notas y cierra con "Prom."; la calificación se abre
            // en Final/Recup./Def. y la última columna queda para la firma del notificado.
            tabla.Header(encabezado =>
            {
                Celda(encabezado.Cell().RowSpan(2), "N°", header: true);
                Celda(encabezado.Cell().RowSpan(2), "APELLIDO Y NOMBRE", header: true, alinearIzquierda: true);
                Celda(encabezado.Cell().ColumnSpan(NotasPorBimestre + 1u), "1er. Bimestre", header: true);
                Celda(encabezado.Cell().ColumnSpan(NotasPorBimestre + 1u), "2do Bimestre", header: true);
                Celda(encabezado.Cell().ColumnSpan(3), "CALIFICACIÓN", header: true);
                Celda(encabezado.Cell().RowSpan(2), "NOTIFICADO", header: true);
                for (var b = 0; b < 2; b++)
                {
                    for (var n = 0; n < NotasPorBimestre; n++)
                    {
                        Celda(encabezado.Cell(), string.Empty, header: true);
                    }

                    Celda(encabezado.Cell(), "Prom.", header: true);
                }

                Celda(encabezado.Cell(), "Final", header: true);
                Celda(encabezado.Cell(), "Recup.", header: true);
                Celda(encabezado.Cell(), "Def.", header: true);
            });

            // Espaciado legacy: 1.03 cm por renglón (~29 pt), más aire que TP.
            Filas(tabla, seccion.Cursando, celdasEnBlanco: TotalColumnasCalificaciones - 2, altoFila: 28);

            if (seccion.Recursantes.Count > 0)
            {
                Subtitulo(tabla, TotalColumnasCalificaciones);
                Filas(tabla, seccion.Recursantes, celdasEnBlanco: TotalColumnasCalificaciones - 2, altoFila: 28);
            }
        });
    }

    private static void Subtitulo(TableDescriptor tabla, int columnas)
    {
        var celda = tabla.Cell().ColumnSpan((uint)columnas)
            .Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(3);
        celda.Text("RECURSANTES").Bold().FontSize(9);
    }

    private static void Filas(
        TableDescriptor tabla, IReadOnlyList<CarpetaComisionAlumnoDto> alumnos, int celdasEnBlanco, float altoFila)
    {
        for (var i = 0; i < alumnos.Count; i++)
        {
            var alumno = alumnos[i];
            Celda(tabla.Cell(), (i + 1).ToString(System.Globalization.CultureInfo.InvariantCulture), altoFila: altoFila);
            Celda(tabla.Cell(), $"{alumno.Apellido}, {alumno.Nombre}", alinearIzquierda: true, altoFila: altoFila);
            for (var d = 0; d < celdasEnBlanco; d++)
            {
                Celda(tabla.Cell(), string.Empty, altoFila: altoFila);   // a completar a mano
            }
        }
    }

    private static void Celda(
        IContainer celda, string texto, bool header = false, bool alinearIzquierda = false, float altoFila = 16)
    {
        var contenido = celda.Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(2);
        if (!alinearIzquierda)
        {
            contenido = contenido.AlignCenter();
        }

        if (header)
        {
            contenido.AlignMiddle().Text(texto).SemiBold().FontSize(7);
        }
        else
        {
            contenido.MinHeight(altoFila).Text(texto).FontSize(8);
        }
    }

    private static void Pie(PageDescriptor pagina, string referencia)
    {
        pagina.Footer().AlignRight().Text(text =>
        {
            text.Span($"Cuatrimestre {referencia}  —  Página ").FontSize(8).FontColor(Colors.Grey.Medium);
            text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
            text.Span(" de ").FontSize(8).FontColor(Colors.Grey.Medium);
            text.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
        });
    }
}
