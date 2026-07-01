namespace Esba.Application.DTOs.Examenes;

/// <summary>
/// Modelo completo del acta volante de una mesa de examen, listo para maquetar
/// (PDF/Excel). Lo arma el handler con la cabecera de la mesa y los candidatos.
/// </summary>
public sealed record ActaMesaModel
{
    public required string Titulo { get; init; }

    public required string CarreraLarga { get; init; }

    public required int Mesa { get; init; }

    public required string TipoExamen { get; init; }

    public required ActaMesaCabeceraDto Cabecera { get; init; }

    public required IReadOnlyList<ActaAlumnoDto> Alumnos { get; init; }
}
