using ClosedXML.Excel;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Asistencias;
using Esba.Domain.Asistencias;
using Esba.Domain.Examenes;

namespace Esba.Infrastructure.Excel;

/// <summary>
/// Exportación a Excel de las carpetas por comisión con ClosedXML (reemplaza el
/// BtnExcel de lstNotasyPractico.pas, que abría por OLE la plantilla
/// Planilla_de_notas.xls y generaba un .xls por comisión). Un único .xlsx con una
/// hoja por comisión y la grilla en blanco según el tipo: TP 1–5 + condición para
/// trabajos prácticos, bimestres + calificación + notificado para la planilla de
/// profesores (a diferencia del legacy, que exportaba siempre el formato de
/// calificaciones sin importar el menú de origen).
/// </summary>
public sealed class CarpetaComisionExcelService : ICarpetaComisionExcelService
{
    private const int CantidadTp = 5;
    private const int NotasPorBimestre = 5;

    public byte[] GenerarCarpeta(CarpetaComisionModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (model.Tipo == TipoCarpetaComision.Asistencia)
        {
            throw new ArgumentException(
                "La carpeta de asistencia no tiene exportación a Excel.", nameof(model));
        }

        using var libro = new XLWorkbook();
        var indice = 0;
        foreach (var seccion in model.Secciones)
        {
            indice++;
            var hoja = libro.AddWorksheet(NombreHojaValido(
                $"{seccion.Cabecera.Cutuco}-{seccion.Cabecera.CodigoMateria}-{indice}"));

            var fila = Cabecera(hoja, model, seccion.Cabecera);

            if (model.Tipo == TipoCarpetaComision.TrabajosPracticos)
            {
                TablaTrabajosPracticos(hoja, ref fila, seccion);
            }
            else
            {
                TablaCalificaciones(hoja, ref fila, seccion);
            }

            // Anchos explícitos: AdjustToContents ignora las celdas combinadas del
            // encabezado y colapsaría las columnas de la grilla, que van en blanco
            // (se completan a mano).
            AnchosGrilla(hoja, model.Tipo);

            // Impresión en una sola hoja apaisada (la grilla es más ancha que un A4
            // vertical y se partiría en dos páginas).
            hoja.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            hoja.PageSetup.FitToPages(1, 0);
        }

        return AGuardar(libro);
    }

    private static int Cabecera(IXLWorksheet hoja, CarpetaComisionModel model, CarpetaComisionCabeceraDto cabecera)
    {
        var titulo = model.Tipo == TipoCarpetaComision.TrabajosPracticos
            ? "CARPETA DE TRABAJOS PRÁCTICOS"
            : "PLANILLA DE CALIFICACIONES";

        var fila = 1;
        Titulo(hoja.Cell(fila++, 1), model.CarreraLarga);
        Titulo(hoja.Cell(fila++, 1), titulo);
        hoja.Cell(fila++, 1).Value = $"Asignatura: {cabecera.DescripcionMateria}";

        if (CodigoComision.TryDescomponer(cabecera.Cutuco, out var codigo))
        {
            hoja.Cell(fila++, 1).Value =
                $"Cuat.: {codigo.Cuatrimestre}    Turno: {codigo.TurnoTexto}    Comisión: {codigo.ComisionTexto}";
        }

        if (!string.IsNullOrWhiteSpace(cabecera.Docente))
        {
            hoja.Cell(fila++, 1).Value = $"Profesor/a: {cabecera.Docente}";
        }

        hoja.Cell(fila++, 1).Value = $"Cuatrimestre: {model.CuatrimestreAnio}    Ciclo lectivo: {model.FechaEmision.Year}";

        return fila + 1;
    }

    private static void TablaTrabajosPracticos(IXLWorksheet hoja, ref int fila, CarpetaComisionSeccion seccion)
    {
        var col = 1;
        Encabezado(hoja.Cell(fila, col++), "N°");
        Encabezado(hoja.Cell(fila, col++), "Código");
        Encabezado(hoja.Cell(fila, col++), "Apellido y nombre");
        for (var t = 0; t < CantidadTp; t++)
        {
            Encabezado(hoja.Cell(fila, col++), $"TP {t + 1}");
        }

        Encabezado(hoja.Cell(fila, col), "Condición");
        var ultimaColumna = col;
        fila++;

        Filas(hoja, ref fila, seccion, ultimaColumna);
    }

    private static void TablaCalificaciones(IXLWorksheet hoja, ref int fila, CarpetaComisionSeccion seccion)
    {
        // Dos filas de encabezado como la plantilla Planilla_de_notas.xls: los
        // bimestres y la calificación agrupan sus columnas con celdas combinadas.
        var col = 1;
        EncabezadoDoble(hoja, fila, col++, "N°");
        EncabezadoDoble(hoja, fila, col++, "Código");
        EncabezadoDoble(hoja, fila, col++, "Apellido y nombre");

        foreach (var bimestre in new[] { "1er. Bimestre", "2do Bimestre" })
        {
            EncabezadoRango(hoja.Range(fila, col, fila, col + NotasPorBimestre), bimestre);
            for (var n = 0; n < NotasPorBimestre; n++)
            {
                Encabezado(hoja.Cell(fila + 1, col++), string.Empty);
            }

            Encabezado(hoja.Cell(fila + 1, col++), "Prom.");
        }

        EncabezadoRango(hoja.Range(fila, col, fila, col + 2), "Calificación");
        Encabezado(hoja.Cell(fila + 1, col++), "Final");
        Encabezado(hoja.Cell(fila + 1, col++), "Recup.");
        Encabezado(hoja.Cell(fila + 1, col++), "Def.");
        EncabezadoDoble(hoja, fila, col, "Notificado");
        var ultimaColumna = col;
        fila += 2;

        Filas(hoja, ref fila, seccion, ultimaColumna);
    }

    private static void AnchosGrilla(IXLWorksheet hoja, TipoCarpetaComision tipo)
    {
        hoja.Column(1).Width = 5;    // N°
        hoja.Column(2).Width = 10;   // Código
        hoja.Column(3).Width = 32;   // Apellido y nombre

        if (tipo == TipoCarpetaComision.TrabajosPracticos)
        {
            hoja.Columns(4, 3 + CantidadTp).Width = 7;   // TP 1..TP 5
            hoja.Column(4 + CantidadTp).Width = 14;      // Condición
            return;
        }

        var col = 4;
        for (var b = 0; b < 2; b++)
        {
            hoja.Columns(col, col + NotasPorBimestre - 1).Width = 4.5;  // notas del bimestre
            col += NotasPorBimestre;
            hoja.Column(col++).Width = 7;                               // Prom.
        }

        hoja.Columns(col, col + 2).Width = 7;                           // Final/Recup./Def.
        hoja.Column(col + 3).Width = 16;                                // Notificado (firma)
    }

    private static void Filas(IXLWorksheet hoja, ref int fila, CarpetaComisionSeccion seccion, int ultimaColumna)
    {
        Nomina(hoja, ref fila, seccion.Cursando, ultimaColumna);

        if (seccion.Recursantes.Count > 0)
        {
            var subtitulo = hoja.Range(fila, 1, fila, ultimaColumna).Merge().FirstCell();
            subtitulo.Value = "RECURSANTES";
            subtitulo.Style.Font.Bold = true;
            fila++;
            Nomina(hoja, ref fila, seccion.Recursantes, ultimaColumna);
        }
    }

    private static void Nomina(
        IXLWorksheet hoja, ref int fila, IReadOnlyList<CarpetaComisionAlumnoDto> alumnos, int ultimaColumna)
    {
        for (var i = 0; i < alumnos.Count; i++)
        {
            var alumno = alumnos[i];
            hoja.Cell(fila, 1).Value = i + 1;
            hoja.Cell(fila, 2).Value = alumno.CodigoAlumno;
            hoja.Cell(fila, 3).Value = $"{alumno.Apellido}, {alumno.Nombre}";
            // Bordes en toda la fila para que la grilla en blanco salga impresa.
            hoja.Range(fila, 1, fila, ultimaColumna).Style
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetInsideBorder(XLBorderStyleValues.Thin);
            fila++;
        }
    }

    private static void Titulo(IXLCell celda, string texto)
    {
        celda.Value = texto;
        celda.Style.Font.Bold = true;
    }

    private static void EncabezadoDoble(IXLWorksheet hoja, int fila, int col, string texto) =>
        EncabezadoRango(hoja.Range(fila, col, fila + 1, col), texto);

    private static void EncabezadoRango(IXLRange rango, string texto)
    {
        rango.Merge();
        rango.FirstCell().Value = texto;
        EstiloEncabezado(rango.Style);
    }

    private static void Encabezado(IXLCell celda, string texto)
    {
        celda.Value = texto;
        EstiloEncabezado(celda.Style);
    }

    private static void EstiloEncabezado(IXLStyle estilo)
    {
        estilo.Font.Bold = true;
        estilo.Fill.BackgroundColor = XLColor.FromHtml("#1E40AF");
        estilo.Font.FontColor = XLColor.White;
        estilo.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }

    private static byte[] AGuardar(XLWorkbook libro)
    {
        using var memoria = new MemoryStream();
        libro.SaveAs(memoria);
        return memoria.ToArray();
    }

    /// <summary>Excel limita el nombre de hoja a 31 chars y prohíbe : \ / ? * [ ].</summary>
    private static string NombreHojaValido(string titulo)
    {
        var limpio = new string((titulo ?? string.Empty)
            .Where(ch => !"\\/?*[]:".Contains(ch, StringComparison.Ordinal)).ToArray()).Trim();
        return limpio.Length switch
        {
            0 => "Carpeta",
            > 31 => limpio[..31],
            _ => limpio,
        };
    }
}
