namespace Esba.Application.Abstractions;

/// <summary>
/// Una fila de carga de final ya resuelta por el caso de uso: la condición
/// resultante y la nota del analítico ya las calculó el handler
/// (Esba.Domain.Examenes.CalculoCondicionFinal); el repositorio solo persiste.
/// </summary>
public sealed record FilaCargaFinalResuelta
{
    public required string CodigoAlumno { get; init; }

    public required string CodigoMateria { get; init; }

    /// <summary>true si la carrera es terciaria (actualiza los 3 finales; bachiller solo el 1°).</summary>
    public required bool EsTerciaria { get; init; }

    public decimal? Nota1 { get; init; }

    public DateOnly? Fecha1 { get; init; }

    public string? Acta1 { get; init; }

    public decimal? Nota2 { get; init; }

    public DateOnly? Fecha2 { get; init; }

    public string? Acta2 { get; init; }

    public decimal? Nota3 { get; init; }

    public DateOnly? Fecha3 { get; init; }

    public string? Acta3 { get; init; }

    public required string NuevaCondicion { get; init; }

    /// <summary>Si no es null, el final aprueba: se mueve a CURSADA_HST + ANALITIC.</summary>
    public decimal? NotaAnalitico { get; init; }

    public DateOnly? FechaAnalitico { get; init; }

    public string? ActaAnalitico { get; init; }
}

/// <summary>
/// Volcado transaccional de las notas de final de una mesa. Porta el SP XXX_MESAS
/// a C# (decisión 2026-06-26: se elimina el staging "$$$PERMEXA"). Por cada fila:
/// UPDATE CURSADA y, si el final aprueba, mover la cursada a CURSADA_HST + insertar
/// en ANALITIC + borrar la cursada y el permiso consumido de PERMEXA.
/// </summary>
public interface ICargaFinalRepository
{
    /// <summary>Confirma todas las filas en una sola transacción. Devuelve cuántas se procesaron.</summary>
    /// <param name="consumirPermiso">
    /// true (por mesa, XXX_MESAS): al aprobar borra el permiso de PERMEXA.
    /// false (por alumno, XXX_CARGA_FINAL): conserva el permiso.
    /// </param>
    Task<int> ConfirmarAsync(
        string codigoCarrera,
        int mesa,
        int codigoUsuario,
        bool consumirPermiso,
        IReadOnlyList<FilaCargaFinalResuelta> filas,
        CancellationToken ct);
}
