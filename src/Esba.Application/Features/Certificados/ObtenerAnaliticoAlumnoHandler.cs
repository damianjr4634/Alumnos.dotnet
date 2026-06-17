using Esba.Application.Abstractions;
using Esba.Application.DTOs.Certificados;

namespace Esba.Application.Features.Certificados;

/// <summary>
/// Lectura del analítico del alumno (materias del plan con su condición + promedio
/// general) para la pantalla de constancia. Sucesor de <c>Constancia()</c> /
/// <c>FormActivate</c> de constanciaalumnos2.pas: solo lectura, sin transacción.
/// </summary>
public sealed class ObtenerAnaliticoAlumnoHandler
{
    private readonly IConstanciaMateriasProcedure _materias;
    private readonly IPromedioGeneralProcedure _promedio;

    public ObtenerAnaliticoAlumnoHandler(IConstanciaMateriasProcedure materias, IPromedioGeneralProcedure promedio)
    {
        _materias = materias;
        _promedio = promedio;
    }

    public async Task<AnaliticoAlumnoModel> ObtenerAsync(string codigoAlumno, string codigoCarrera, CancellationToken ct)
    {
        var materias = await _materias.ListarAsync(codigoAlumno, codigoCarrera, ct).ConfigureAwait(false);
        var promedio = await _promedio.ObtenerAsync(codigoAlumno, codigoCarrera, ct).ConfigureAwait(false);

        return new AnaliticoAlumnoModel { Materias = materias, PromedioGeneral = promedio };
    }
}
