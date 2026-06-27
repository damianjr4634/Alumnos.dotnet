namespace Esba.Application.DTOs.Examenes;

/// <summary>
/// La nota de final cargada para un alumno/materia de la mesa (lo editado en la
/// grilla). Las notas vacías van null. La condición resultante NO viaja desde la
/// UI: la recalcula el servidor a partir de estas notas (autoritativo, §2.1).
/// </summary>
public sealed record NotaFinalAlumnoInput
{
    public required string CodigoAlumno { get; init; }

    public required string CodigoMateria { get; init; }

    /// <summary>Condición actual del alumno (CURSADA.CONDICION) sobre la que se decide la nueva.</summary>
    public required string CondicionActual { get; init; }

    public decimal? Nota1 { get; init; }

    public DateOnly? Fecha1 { get; init; }

    public string? Acta1 { get; init; }

    public decimal? Nota2 { get; init; }

    public DateOnly? Fecha2 { get; init; }

    public string? Acta2 { get; init; }

    public decimal? Nota3 { get; init; }

    public DateOnly? Fecha3 { get; init; }

    public string? Acta3 { get; init; }
}

/// <summary>
/// Confirmación de la carga de notas de final de una mesa (sucesor del
/// XXX_MESAS que el legacy disparaba al cerrar FinalesxMesayComision). Vuelca,
/// en una transacción, las notas de todos los alumnos cargados de la mesa.
/// El staging "$$$PERMEXA" del legacy se reemplaza por <see cref="Filas"/> en
/// memoria (decisión 2026-06-26: se elimina el estado global, §2.3).
/// </summary>
public sealed record CargaNotasFinalCommand
{
    public required string CodigoCarrera { get; init; }

    public int Mesa { get; init; }

    /// <summary>Tipo de la carrera (TER / BAC / BAD): decide el cálculo de condición.</summary>
    public required string TipoCarrera { get; init; }

    public required int CodigoUsuario { get; init; }

    public IReadOnlyList<NotaFinalAlumnoInput> Filas { get; init; } = [];
}
