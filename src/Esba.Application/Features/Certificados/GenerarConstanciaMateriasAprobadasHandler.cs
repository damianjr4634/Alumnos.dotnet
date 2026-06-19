using System.Globalization;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Alumnos;
using Esba.Application.DTOs.Certificados;
using Esba.Domain.Certificados;
using Esba.Domain.Common;

namespace Esba.Application.Features.Certificados;

/// <summary>
/// Emite la "Constancia de Materias Aprobadas" (reporte tabular del analítico),
/// sucesor de <c>BitBtn1Click</c> de constanciaalumnos2.pas. Es de solo lectura (no
/// abre transacción): junta los datos del alumno y la carrera, lista las materias del
/// plan con su condición y delega el armado del PDF al servicio de reporte.
/// </summary>
public sealed class GenerarConstanciaMateriasAprobadasHandler
{
    private readonly IAlumnosQuery _alumnos;
    private readonly IConstanciasQuery _carreras;
    private readonly IConstanciaMateriasProcedure _materias;
    private readonly IConstanciaAnaliticoReportService _reporte;
    private readonly TimeProvider _tiempo;

    public GenerarConstanciaMateriasAprobadasHandler(
        IAlumnosQuery alumnos,
        IConstanciasQuery carreras,
        IConstanciaMateriasProcedure materias,
        IConstanciaAnaliticoReportService reporte,
        TimeProvider tiempo)
    {
        _alumnos = alumnos;
        _carreras = carreras;
        _materias = materias;
        _reporte = reporte;
        _tiempo = tiempo;
    }

    public async Task<Result<byte[]>> GenerarPdfAsync(
        string codigoAlumno,
        string codigoCarrera,
        string? anteQuien,
        bool incluirMembrete,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(anteQuien))
        {
            return Result.Error<byte[]>("Indique ante quién se presenta la constancia.");
        }

        var alumno = await _alumnos.ObtenerDetalleAsync(codigoCarrera, codigoAlumno, ct).ConfigureAwait(false);
        if (alumno is null)
        {
            return Result.Error<byte[]>("El alumno no existe.");
        }

        var carrera = await _carreras.ObtenerDatosCarreraAsync(codigoCarrera, ct).ConfigureAwait(false);
        if (carrera is null)
        {
            return Result.Error<byte[]>("La carrera no existe.");
        }

        var materiasDto = await _materias.ListarAsync(codigoAlumno, codigoCarrera, ct).ConfigureAwait(false);

        var filas = ConstanciaMateriasAprobadasFormatter.Formatear(
            materiasDto.Select(m => new MateriaAnaliticoConstancia
            {
                Cuatrimestre = m.Cuatrimestre,
                Descripcion = m.Descripcion ?? string.Empty,
                EsAnual = m.Anual == "*",
                Condicion = m.Condicion,
                Nota = m.Nota,
                Fecha = m.Fecha,
                Instituto = m.Instituto,
                Caracteristica = m.Caracteristica,
                ActividadInterna = m.ActividadInterna,
                ActividadDgegp = m.ActividadDgegp,
                EximidoDescripcion = m.EximidoDescripcion,
            }).ToList());

        var model = new ConstanciaMateriasAprobadasModel
        {
            Introduccion = ComponerIntroduccion(alumno, carrera),
            Filas = filas,
            AnteQuien = anteQuien.Trim(),
            IncluirMembrete = incluirMembrete,
            Instituto = carrera.Instituto,
            Caracteristica = carrera.Caracteristica,
            Secretaria = carrera.Secretaria,
            Rector = carrera.Rector,
        };

        return Result.Ok(_reporte.GenerarMateriasAprobadas(model));
    }

    private string ComponerIntroduccion(AlumnoDetailDto alumno, CarreraConstanciaDto carrera)
    {
        var hoy = DateOnly.FromDateTime(_tiempo.GetLocalNow().DateTime);
        var nombre = $"{alumno.Apellido}, {alumno.Nombre}".Trim().ToUpperInvariant();
        var codigo = TextoCastellano.CodigoConPuntos(alumno.Codigo);
        var carreraNombre = (carrera.Nombre ?? string.Empty).Trim().ToUpperInvariant();

        return $"En Buenos Aires a los {hoy.Day} días del mes de {TextoCastellano.MesEnLetras(hoy.Month)} " +
               $"de {hoy.Year.ToString(CultureInfo.InvariantCulture)} se extiende la presente perteneciente al " +
               $"alumno/a {nombre} - {codigo} correspondiente a la carrera {carreraNombre}.";
    }
}
