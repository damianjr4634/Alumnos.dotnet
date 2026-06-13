namespace Esba.Application.Abstractions;

/// <summary>
/// Wrapper tipado del SP legacy XXX_NUMCUATANIO (usado por
/// FuncionesText.Cuatrimestre): devuelve el cuatrimestre vigente de una
/// carrera en formato CUA_ANIO ("124" = 1º/2024), calculado desde
/// TBL_CUAT/TBL_TRIM según la fecha actual y la modalidad de la carrera.
/// </summary>
public interface ICuatrimestreVigenteProcedure
{
    /// <summary>Null si la carrera no tiene período vigente configurado.</summary>
    Task<string?> ObtenerAsync(string codigoCarrera, CancellationToken ct);
}
