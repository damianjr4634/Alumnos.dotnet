using Esba.Application.Common;

namespace Esba.Application.Abstractions;

/// <summary>
/// Exportación de un listado a Excel. Sucesor de FuncionesExcel.pas (automación
/// OLE de Excel): la implementación usa ClosedXML y no requiere Excel instalado
/// (migration_improvements.md §3.5).
/// </summary>
public interface IExcelExportService
{
    /// <summary>
    /// Genera un .xlsx con una hoja: encabezados desde <paramref name="columnas"/>
    /// y una fila por cada elemento de <paramref name="filas"/>.
    /// </summary>
    byte[] Exportar<T>(
        IReadOnlyList<T> filas,
        IReadOnlyList<ColumnaExportable<T>> columnas,
        string titulo);
}
