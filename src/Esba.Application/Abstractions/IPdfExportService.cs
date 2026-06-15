using Esba.Application.Common;

namespace Esba.Application.Abstractions;

/// <summary>
/// Exportación de un listado a PDF en formato tabla. Sucesor del "Imprimir" de
/// modulovariable (Gnostice eDocEngine + GDI); la implementación usa QuestPDF
/// (migration_improvements.md §3.5). Los reportes con formato propio (hito 9)
/// son un servicio aparte: esto es solo el volcado tabular de un listado.
/// </summary>
public interface IPdfExportService
{
    /// <summary>
    /// Genera un PDF apaisado con título y una tabla: encabezados desde
    /// <paramref name="columnas"/> y una fila por elemento de <paramref name="filas"/>.
    /// </summary>
    byte[] ExportarTabla<T>(
        IReadOnlyList<T> filas,
        IReadOnlyList<ColumnaExportable<T>> columnas,
        string titulo);
}
