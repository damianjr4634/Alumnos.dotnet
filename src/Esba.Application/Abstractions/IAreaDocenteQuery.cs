using Esba.Application.DTOs.Docente;

namespace Esba.Application.Abstractions;

/// <summary>
/// Lecturas del área docente (hito 19): lo que un docente tiene a cargo. El alcance
/// de datos es el CODPROFES del claim del usuario logueado; estas queries nunca
/// reciben el docente desde la UI sin pasar por ese claim.
/// </summary>
public interface IAreaDocenteQuery
{
    /// <summary>
    /// Comisiones donde el docente es el titular (COMARM.CODPROFES), todas las carreras
    /// y períodos, de la más reciente a la más vieja, con la cantidad de alumnos
    /// cursando/recursando y el estado de su precarga (null = sin carga).
    /// </summary>
    Task<IReadOnlyList<ComisionDocenteDto>> ListarComisionesAsync(string codigoDocente, CancellationToken ct);

    /// <summary>
    /// Mesas donde el docente es el titular (MESAS.TITULAR), de la más próxima a la más
    /// vieja; <paramref name="desde"/> recorta las anteriores a esa fecha (null = todas).
    /// Incluye la cantidad de alumnos con permiso y el estado de su precarga.
    /// </summary>
    Task<IReadOnlyList<MesaDocenteDto>> ListarMesasAsync(string codigoDocente, DateOnly? desde, CancellationToken ct);
}
