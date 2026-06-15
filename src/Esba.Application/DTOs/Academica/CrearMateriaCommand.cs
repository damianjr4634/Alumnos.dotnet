namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Alta de una materia (sucesor del INSERT de altamodifmaterias.BotonGrabarClick
/// en modo "Nuevo"). El código forma parte de la PK y nace normalizado a 2
/// dígitos; la materia nace activa salvo que se marque la baja.
/// </summary>
public sealed record CrearMateriaCommand : IMateriaCampos
{
    public required string CodigoCarrera { get; init; }

    public required string Codigo { get; init; }

    public string? Nombre { get; init; }

    public string? Sigla { get; init; }

    public short Cuatrimestre { get; init; }

    public short Orden { get; init; }

    public bool EsAnual { get; init; }

    public bool AdmitePromocion { get; init; }

    public bool ApruebaSinFinal { get; init; }

    public string? CodigoEquivalencia { get; init; }

    public IReadOnlyList<string> CorrelativasCursada { get; init; } = [];

    public IReadOnlyList<string> CorrelativasFinal { get; init; } = [];

    public bool DadaDeBaja { get; init; }
}
