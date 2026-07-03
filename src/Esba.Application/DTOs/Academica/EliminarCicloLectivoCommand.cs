namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Eliminación de un año lectivo de TBL_CUAT o TBL_TRIM (en la grilla del
/// legacy se borraba la fila y el grabado reinsertaba el resto).
/// </summary>
public sealed record EliminarCicloLectivoCommand
{
    public required int Anio { get; init; }

    /// <summary>true = TBL_TRIM (trimestral "uso 333"); false = TBL_CUAT.</summary>
    public required bool Trimestral { get; init; }
}
