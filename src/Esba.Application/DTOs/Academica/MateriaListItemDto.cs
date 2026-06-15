namespace Esba.Application.DTOs.Academica;

/// <summary>Fila de la grilla de materias (sucesor de altamodifmaterias.pas).</summary>
public sealed record MateriaListItemDto
{
    public required string Codigo { get; init; }

    public required string CodigoCarrera { get; init; }

    public string? Nombre { get; init; }

    public string? Sigla { get; init; }

    public short? Cuatrimestre { get; init; }

    public bool? EsAnual { get; init; }

    public bool? AdmitePromocion { get; init; }

    public short? Orden { get; init; }

    /// <summary>ESTADO CHAR(1): 'B' = dada de baja, 'Y'/otro = activa (legacy altamodifmaterias).</summary>
    public string? Estado { get; init; }

    /// <summary>Conveniencia para la UI: la materia está dada de baja.</summary>
    public bool DadaDeBaja => string.Equals(Estado?.Trim(), "B", StringComparison.OrdinalIgnoreCase);
}
