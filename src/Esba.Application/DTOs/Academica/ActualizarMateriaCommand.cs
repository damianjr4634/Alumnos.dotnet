namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Modificación de una materia (sucesor del UPDATE de
/// altamodifmaterias.BotonGrabarClick en modo "Modificar"). El código y la
/// carrera identifican la fila y no cambian; el resto de los campos se reemplaza.
/// </summary>
public sealed record ActualizarMateriaCommand : IMateriaCampos
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
