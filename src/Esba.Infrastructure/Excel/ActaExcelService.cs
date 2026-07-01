using ClosedXML.Excel;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Examenes;
using Esba.Domain.Certificados;
using Esba.Domain.Examenes;

namespace Esba.Infrastructure.Excel;

/// <summary>
/// Exportación de las actas de examen a Excel con ClosedXML (reemplaza la
/// automatización OLE con plantilla .xls de las pantallas legacy de actas, §3.5).
/// Una hoja por comisión/mesa, con la cabecera institucional y la grilla de alumnos
/// con columnas de calificación en blanco para completar.
/// </summary>
public sealed class ActaExcelService : IActaExcelService
{
    public byte[] GenerarActaComision(ActaComisionModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        using var libro = new XLWorkbook();
        var indice = 0;
        foreach (var seccion in model.Secciones)
        {
            indice++;
            var nombre = NombreHojaValido($"{seccion.Cabecera.Cutuco}-{seccion.Cabecera.CodigoMateria}-{indice}");
            var hoja = libro.AddWorksheet(nombre);

            var fila = Cabecera(hoja, model.CarreraLarga, model.Titulo, seccion.Cabecera.DescripcionMateria,
                seccion.Cabecera.Cutuco, seccion.Cabecera.Docente);

            TablaAlumnos(hoja, ref fila, seccion.Alumnos, conPermiso: false);
            hoja.Columns().AdjustToContents();
        }

        return AGuardar(libro);
    }

    public byte[] GenerarActaMesa(ActaMesaModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        using var libro = new XLWorkbook();
        var hoja = libro.AddWorksheet(NombreHojaValido($"Mesa {model.Mesa}"));

        var fila = Cabecera(hoja, model.CarreraLarga, model.Titulo, model.Cabecera.DescripcionMateria,
            model.Cabecera.Cutuco, model.Cabecera.Docente);

        hoja.Cell(fila, 1).Value = $"Mesa: {model.Mesa}";
        fila += 2;

        TablaAlumnos(hoja, ref fila, model.Alumnos, conPermiso: true);
        hoja.Columns().AdjustToContents();

        return AGuardar(libro);
    }

    private static int Cabecera(
        IXLWorksheet hoja, string carreraLarga, string titulo, string? asignatura, int? cutuco, string? docente)
    {
        var fila = 1;
        Titulo(hoja.Cell(fila++, 1), carreraLarga);
        Titulo(hoja.Cell(fila++, 1), titulo);
        hoja.Cell(fila++, 1).Value = $"Asignatura: {asignatura}";

        if (cutuco.HasValue && CodigoComision.TryDescomponer(cutuco.Value, out var codigo))
        {
            hoja.Cell(fila++, 1).Value =
                $"Cuat.: {codigo.Cuatrimestre}    Turno: {codigo.TurnoTexto}    Comisión: {codigo.ComisionTexto}";
        }

        if (!string.IsNullOrWhiteSpace(docente))
        {
            hoja.Cell(fila++, 1).Value = $"Docente/s: {docente}";
        }

        return fila + 1;
    }

    private static void TablaAlumnos(
        IXLWorksheet hoja, ref int fila, IReadOnlyList<ActaAlumnoDto> alumnos, bool conPermiso)
    {
        var col = 1;
        if (conPermiso)
        {
            Encabezado(hoja.Cell(fila, col++), "Permiso");
        }

        Encabezado(hoja.Cell(fila, col++), "Código");
        Encabezado(hoja.Cell(fila, col++), "Apellido y nombre");
        Encabezado(hoja.Cell(fila, col++), "Calificación");
        Encabezado(hoja.Cell(fila, col), "En letras");
        fila++;

        foreach (var alumno in alumnos)
        {
            col = 1;
            if (conPermiso)
            {
                hoja.Cell(fila, col++).Value = alumno.PermisoExamen ?? 0;
            }

            hoja.Cell(fila, col++).Value = alumno.CodigoAlumno;
            hoja.Cell(fila, col).Value = $"{alumno.Apellido}, {alumno.Nombre}";
            // Calificación y "en letras" quedan en blanco para completar a mano.
            fila++;
        }
    }

    private static void Titulo(IXLCell celda, string texto)
    {
        celda.Value = texto;
        celda.Style.Font.Bold = true;
    }

    private static void Encabezado(IXLCell celda, string texto)
    {
        celda.Value = texto;
        celda.Style.Font.Bold = true;
        celda.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E40AF");
        celda.Style.Font.FontColor = XLColor.White;
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
            0 => "Acta",
            > 31 => limpio[..31],
            _ => limpio,
        };
    }
}
