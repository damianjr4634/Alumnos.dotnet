namespace Esba.Domain.Common;

/// <summary>
/// Estado de una operación de negocio. Modela la semántica ERRCOD/ERRMSG de los
/// SP legacy XXX_* (migration_improvements.md §1.3): 2 → Error, 1 → NeedsConfirmation,
/// 0 con mensaje → Warning, sin código → Ok.
/// </summary>
public enum OperationStatus
{
    Ok = 0,
    Warning = 1,
    NeedsConfirmation = 2,
    Error = 3,
}
