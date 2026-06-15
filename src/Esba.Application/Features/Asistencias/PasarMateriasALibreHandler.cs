using Esba.Application.Abstractions;
using Esba.Domain.Common;

namespace Esba.Application.Features.Asistencias;

/// <summary>
/// Pasa todas las materias CURSANDO del alumno a LIBRES (sucesor de
/// XXX_FALTAS_PASLIBRE), con el patrón de dos fases: previsualizar (rollback,
/// devuelve el mensaje de confirmación) y confirmar (commit).
/// </summary>
public sealed class PasarMateriasALibreHandler
{
    private readonly IPaseLibreProcedure _procedimiento;

    public PasarMateriasALibreHandler(IPaseLibreProcedure procedimiento)
    {
        _procedimiento = procedimiento;
    }

    public Task<Result<string>> PrevisualizarAsync(string codigoAlumno, string codigoCarrera, CancellationToken ct) =>
        Ejecutar(codigoAlumno, codigoCarrera, confirmar: false, ct);

    public Task<Result<string>> ConfirmarAsync(string codigoAlumno, string codigoCarrera, CancellationToken ct) =>
        Ejecutar(codigoAlumno, codigoCarrera, confirmar: true, ct);

    private Task<Result<string>> Ejecutar(string codigoAlumno, string codigoCarrera, bool confirmar, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(codigoAlumno) || string.IsNullOrWhiteSpace(codigoCarrera))
        {
            return Task.FromResult(Result.Error<string>("Alumno o carrera inválidos."));
        }

        return _procedimiento.EjecutarAsync(codigoAlumno, codigoCarrera, confirmar, ct);
    }
}
