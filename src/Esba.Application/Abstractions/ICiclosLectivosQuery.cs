using Esba.Application.DTOs.Academica;

namespace Esba.Application.Abstractions;

/// <summary>Lecturas de los ciclos lectivos (TBL_CUAT/TBL_TRIM) para la pantalla de cuatrimestres.</summary>
public interface ICiclosLectivosQuery
{
    /// <summary>Todos los años de TBL_CUAT, del más reciente al más viejo.</summary>
    Task<IReadOnlyList<CicloCuatrimestralDto>> ListarCuatrimestralesAsync(CancellationToken ct);

    /// <summary>Todos los años de TBL_TRIM ("uso 333"), del más reciente al más viejo.</summary>
    Task<IReadOnlyList<CicloTrimestralDto>> ListarTrimestralesAsync(CancellationToken ct);
}
