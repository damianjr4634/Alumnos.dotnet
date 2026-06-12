namespace Esba.Domain.Common;

/// <summary>
/// Resultado de una operación de negocio. Los errores esperables viajan acá,
/// no como excepciones (migration_improvements.md §2.5).
/// </summary>
public sealed record Result<T>
{
    public OperationStatus Status { get; init; }

    public string? Message { get; init; }

    public T? Value { get; init; }

    /// <summary>La operación completó sus efectos (Ok o Warning).</summary>
    public bool IsSuccess => Status is OperationStatus.Ok or OperationStatus.Warning;
}

/// <summary>Factories de <see cref="Result{T}"/>.</summary>
public static class Result
{
    public static Result<T> Ok<T>(T value) =>
        new() { Status = OperationStatus.Ok, Value = value };

    public static Result<T> Warning<T>(T value, string message) =>
        new() { Status = OperationStatus.Warning, Value = value, Message = message };

    public static Result<T> NeedsConfirmation<T>(string message) =>
        new() { Status = OperationStatus.NeedsConfirmation, Message = message };

    public static Result<T> Error<T>(string message) =>
        new() { Status = OperationStatus.Error, Message = message };

    /// <summary>
    /// Mapea el par ERRCOD/ERRMSG (o FERRCOD/FERRMSG) devuelto por un SP legacy,
    /// respetando la semántica de FuncionesDB.ExecScriptMsg
    /// (migration_improvements.md §1.3): 2 → Error, 1 → NeedsConfirmation,
    /// 0 con mensaje → Warning, sin código → Ok.
    /// </summary>
    public static Result<T> DesdeErrCod<T>(int? errCod, string? errMsg, T value) =>
        errCod switch
        {
            2 => Error<T>(errMsg ?? "Error no especificado por el procedimiento."),
            1 => NeedsConfirmation<T>(errMsg ?? string.Empty),
            0 when !string.IsNullOrWhiteSpace(errMsg) => Warning(value, errMsg),
            _ => Ok(value),
        };
}
