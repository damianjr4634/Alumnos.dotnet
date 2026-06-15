using Esba.Domain.Common;
using Esba.Domain.Entities;

namespace Esba.Application.Abstractions;

public interface IComisionRepository
{
    /// <summary>Busca por PK compuesta (CARRE, CUTUCO, COD_MAT, CUA_ANIO), trackeada.</summary>
    Task<Comision?> ObtenerAsync(string codigoCarrera, short cutuco, string codigoMateria, string cuatrimestreAnio, CancellationToken ct);

    /// <summary>
    /// Inserta o actualiza la comisión y la valida con XXX_VAL_COMISION dentro de
    /// la MISMA transacción (el SP lee la fila recién grabada). Si el SP devuelve
    /// error (superposición horaria o cuatrimestre que no coincide con la
    /// materia) hace rollback y devuelve el mensaje; si no, commitea.
    /// </summary>
    Task<Result<string>> GuardarYValidarAsync(Comision comision, bool esAlta, CancellationToken ct);

    void Eliminar(Comision comision);
}
