using Esba.Application.Abstractions;
using Esba.Application.DTOs.Certificados;
using Esba.Domain.Certificados;
using Esba.Domain.Common;

namespace Esba.Application.Features.Certificados;

/// <summary>
/// Emite la Constancia de Examen Final (CE) de una materia, sucesor de
/// <c>Impresion_Constancia_Examen</c> de constanciaalumnos2.pas. Es de solo lectura:
/// el legacy no usa SP de validación (Query vacío); la única regla es que la condición
/// de la materia sea elegible. El servidor es autoritativo (§2.7): re-deriva la
/// condición desde el wrapper en vez de confiar en lo que vio la pantalla. Reusa el
/// reporte de la constancia de texto (<see cref="IConstanciaReportService"/>).
/// </summary>
public sealed class GenerarConstanciaExamenFinalHandler
{
    private const string Titulo = "CONSTANCIA DE EXAMEN FINAL";
    private const string NotaLegal =
        "La presente certificación carecerá de valor si no estuviera firmada por las autoridades competentes.";

    private readonly IConstanciaMateriasProcedure _materias;
    private readonly IConstanciasQuery _carreras;
    private readonly IParrafoConstanciaProcedure _parrafo;
    private readonly IConstanciaReportService _reporte;

    public GenerarConstanciaExamenFinalHandler(
        IConstanciaMateriasProcedure materias,
        IConstanciasQuery carreras,
        IParrafoConstanciaProcedure parrafo,
        IConstanciaReportService reporte)
    {
        _materias = materias;
        _carreras = carreras;
        _parrafo = parrafo;
        _reporte = reporte;
    }

    public async Task<Result<byte[]>> GenerarPdfAsync(
        string codigoAlumno,
        string codigoCarrera,
        string codigoMateria,
        string? anteQuien,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(anteQuien))
        {
            return Result.Error<byte[]>("Indique ante quién se presenta la constancia.");
        }

        var codMat = (codigoMateria ?? string.Empty).Trim();
        var materias = await _materias.ListarAsync(codigoAlumno, codigoCarrera, ct).ConfigureAwait(false);
        var materia = materias.FirstOrDefault(m => string.Equals(m.CodigoMateria?.Trim(), codMat, StringComparison.OrdinalIgnoreCase));
        if (materia is null)
        {
            return Result.Error<byte[]>("La materia no pertenece al plan del alumno.");
        }

        if (!ConstanciaExamenFinal.EsCondicionElegible(materia.Condicion))
        {
            return Result.Error<byte[]>("En esta condición no se puede imprimir la constancia.");
        }

        var carrera = await _carreras.ObtenerDatosCarreraAsync(codigoCarrera, ct).ConfigureAwait(false);
        if (carrera is null)
        {
            return Result.Error<byte[]>("La carrera no existe.");
        }

        // El TIPO de XXX_PARRAFO_CONSTANCIA para examen final es 'CE-<codmat>'.
        var parrafo = await _parrafo
            .ObtenerAsync(codigoAlumno, codigoCarrera, $"CE-{codMat}", ct)
            .ConfigureAwait(false);

        var model = new ConstanciaAlumnoModel
        {
            Titulo = Titulo,
            Parrafo = parrafo,
            MateriasQueAdeuda = null,
            ParrafoCierre = $"Para ser presentada ante: {anteQuien.Trim()}",
            NotasLegales = [NotaLegal],
            Secretaria = carrera.Secretaria,
            Rector = carrera.Rector,
            Instituto = carrera.Instituto,
            Caracteristica = carrera.Caracteristica,
            NombreCarrera = carrera.Nombre,
        };

        return Result.Ok(_reporte.GenerarConstanciaAlumno(model));
    }
}
