namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Materia completa para precargar el formulario de edición (sucesor del
/// BotonModificarClick de altamodifmaterias.pas). Las correlativas vienen ya
/// separadas en listas de códigos (en la BD se guardan unidas por '-').
/// </summary>
public sealed record MateriaDetailDto
{
    public required string Codigo { get; init; }

    public required string CodigoCarrera { get; init; }

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
