using ClosedXML.Excel;
using Esba.Application.Abstractions;
using Esba.Application.Common;

namespace Esba.Infrastructure.Excel;

/// <summary>
/// Exportación a Excel con ClosedXML (reemplaza la automación OLE de
/// FuncionesExcel.pas, §3.5). Servicio sin estado; seguro como Scoped/Singleton.
/// </summary>
public sealed class ClosedXmlExportService : IExcelExportService
{
    public byte[] Exportar<T>(
        IReadOnlyList<T> filas,
        IReadOnlyList<ColumnaExportable<T>> columnas,
        string titulo)
    {
        ArgumentNullException.ThrowIfNull(filas);
        ArgumentNullException.ThrowIfNull(columnas);

        using var libro = new XLWorkbook();
        var hoja = libro.AddWorksheet(NombreHojaValido(titulo));

        // Encabezados.
        for (var c = 0; c < columnas.Count; c++)
        {
            var celda = hoja.Cell(1, c + 1);
            celda.Value = columnas[c].Titulo;
            celda.Style.Font.Bold = true;
            celda.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E40AF");
            celda.Style.Font.FontColor = XLColor.White;
        }

        // Filas.
        for (var r = 0; r < filas.Count; r++)
        {
            for (var c = 0; c < columnas.Count; c++)
            {
                var columna = columnas[c];
                var celda = hoja.Cell(r + 2, c + 1);
                EscribirValor(celda, columna, filas[r]);
                if (columna.AlinearDerecha)
                {
                    celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                }
            }
        }

        if (columnas.Count > 0)
        {
            hoja.Range(1, 1, Math.Max(filas.Count + 1, 1), columnas.Count)
                .SetAutoFilter();
            hoja.Columns().AdjustToContents();
            hoja.SheetView.FreezeRows(1);
        }

        using var memoria = new MemoryStream();
        libro.SaveAs(memoria);
        return memoria.ToArray();
    }

    /// <summary>
    /// Escribe el valor tipado para que Excel lo trate como número/fecha cuando
    /// corresponde; el resto cae al texto ya formateado por la columna.
    /// </summary>
    private static void EscribirValor<T>(IXLCell celda, ColumnaExportable<T> columna, T fila)
    {
        var valor = columna.Valor(fila);
        switch (valor)
        {
            case null:
                celda.Value = string.Empty;
                break;
            case DateOnly fecha:
                celda.Value = fecha.ToDateTime(TimeOnly.MinValue);
                celda.Style.DateFormat.Format = columna.Formato ?? "dd/MM/yyyy";
                break;
            case DateTime fechaHora:
                celda.Value = fechaHora;
                celda.Style.DateFormat.Format = columna.Formato ?? "dd/MM/yyyy";
                break;
            case bool b:
                celda.Value = b ? "Sí" : "No";
                break;
            case sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal:
                celda.Value = Convert.ToDecimal(valor, System.Globalization.CultureInfo.InvariantCulture);
                if (columna.Formato is not null)
                {
                    celda.Style.NumberFormat.Format = columna.Formato;
                }

                break;
            default:
                celda.Value = columna.Formatear(fila);
                break;
        }
    }

    /// <summary>Excel limita el nombre de hoja a 31 chars y prohíbe : \ / ? * [ ].</summary>
    private static string NombreHojaValido(string titulo)
    {
        if (string.IsNullOrWhiteSpace(titulo))
        {
            return "Listado";
        }

        var limpio = new string(titulo.Where(ch => !"\\/?*[]:".Contains(ch, StringComparison.Ordinal)).ToArray());
        limpio = limpio.Trim();
        return limpio.Length switch
        {
            0 => "Listado",
            > 31 => limpio[..31],
            _ => limpio,
        };
    }
}
