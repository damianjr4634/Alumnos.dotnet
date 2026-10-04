using Esba.Application.DTOs.Academica;
using Esba.Application.DTOs.AreaDocente;
using Esba.Application.DTOs.Examenes;

namespace Esba.Application.Features.AreaDocente;

/// <summary>Resultado de volcar una precarga sobre las filas de una pantalla de secretaría.</summary>
/// <param name="Filas">Filas con los valores del docente aplicados.</param>
/// <param name="Aplicadas">Alumnos a los que se les tomó algo.</param>
/// <param name="SinPrecarga">Alumnos de la pantalla para los que el docente no cargó nada.</param>
/// <param name="Ausentes">Solo mesas: alumnos marcados ausentes por el docente (no se les pone nota).</param>
public sealed record PrecargaAplicada<T>(IReadOnlyList<T> Filas, int Aplicadas, int SinPrecarga, int Ausentes = 0);

/// <summary>
/// "Tomar valores del docente": vuelca la precarga (DOC_CARGA_*) sobre las filas que secretaría
/// ya tiene en pantalla, respetando qué campos edita cada variante de regularización (hito 15)
/// o la carga de finales (hito 14). No decide nada: la condición la siguen calculando los
/// handlers existentes cuando secretaría confirma. Solo se tocan los alumnos con detalle.
/// </summary>
public static class PrecargaAplicador
{
    public static PrecargaAplicada<RegularizacionCursadaDto> Terciaria(
        IReadOnlyList<RegularizacionCursadaDto> filas, CargaComisionDocenteDto precarga)
    {
        ArgumentNullException.ThrowIfNull(filas);
        ArgumentNullException.ThrowIfNull(precarga);
        var detalles = Detalles(precarga);
        var aplicadas = 0;
        var resultado = filas.Select(f =>
        {
            if (!detalles.TryGetValue(f.CodigoAlumno.Trim(), out var d))
            {
                return f;
            }

            aplicadas++;
            return f with
            {
                TpEva = d.Evaluacion1,
                TpEva2 = d.Evaluacion2,
                Recuperatorio = d.Recuperatorio,
                TotalHoras = d.TotalHoras,
                Inasistencias = d.Inasistencias,
                Justificadas = d.Justificadas,
            };
        }).ToList();
        return new(resultado, aplicadas, filas.Count - aplicadas);
    }

    public static PrecargaAplicada<RegularizacionBachilleratoDto> Bachillerato(
        IReadOnlyList<RegularizacionBachilleratoDto> filas, CargaComisionDocenteDto precarga)
    {
        ArgumentNullException.ThrowIfNull(filas);
        ArgumentNullException.ThrowIfNull(precarga);
        var detalles = Detalles(precarga);
        var aplicadas = 0;
        var resultado = filas.Select(f =>
        {
            if (!detalles.TryGetValue(f.CodigoAlumno.Trim(), out var d))
            {
                return f;
            }

            aplicadas++;
            return f with
            {
                TpEva = d.Evaluacion1,
                TpEva2 = d.Evaluacion2,
                Recuperatorio = d.Recuperatorio,
                NotaRegular = d.NotaRegular,
                TotalHoras = d.TotalHoras,
                Inasistencias = d.Inasistencias,
                Justificadas = d.Justificadas,
            };
        }).ToList();
        return new(resultado, aplicadas, filas.Count - aplicadas);
    }

    public static PrecargaAplicada<Regularizacion333Dto> Secundario(
        IReadOnlyList<Regularizacion333Dto> filas, CargaComisionDocenteDto precarga)
    {
        ArgumentNullException.ThrowIfNull(filas);
        ArgumentNullException.ThrowIfNull(precarga);
        var detalles = Detalles(precarga);
        var aplicadas = 0;
        var resultado = filas.Select(f =>
        {
            if (!detalles.TryGetValue(f.CodigoAlumno.Trim(), out var d))
            {
                return f;
            }

            aplicadas++;
            // Diciembre/marzo y sus fechas son de secretaría: no se tocan.
            return f with
            {
                TpEva = d.Evaluacion1,
                TpEva2 = d.Evaluacion2,
                TpEva3 = d.Evaluacion3,
                TotalHoras = d.TotalHoras,
                Inasistencias = d.Inasistencias,
            };
        }).ToList();
        return new(resultado, aplicadas, filas.Count - aplicadas);
    }

    public static PrecargaAplicada<RegularizacionCnaDto> Cna(
        IReadOnlyList<RegularizacionCnaDto> filas, CargaComisionDocenteDto precarga)
    {
        ArgumentNullException.ThrowIfNull(filas);
        ArgumentNullException.ThrowIfNull(precarga);
        var detalles = Detalles(precarga);
        var aplicadas = 0;
        var resultado = filas.Select(f =>
        {
            if (!detalles.TryGetValue(f.CodigoAlumno.Trim(), out var d))
            {
                return f;
            }

            aplicadas++;
            return f with { NotaFinal = d.NotaFinal };
        }).ToList();
        return new(resultado, aplicadas, filas.Count - aplicadas);
    }

    /// <summary>
    /// Mesa: la nota del docente va al llamado vigente del alumno (NumeroFinal 1..3, como edita
    /// secretaría) con la fecha de la mesa; los ausentes no reciben nota (secretaría decide).
    /// </summary>
    public static PrecargaAplicada<CargaFinalAlumnoDto> Mesa(
        IReadOnlyList<CargaFinalAlumnoDto> filas, CargaMesaDocenteDto precarga)
    {
        ArgumentNullException.ThrowIfNull(filas);
        ArgumentNullException.ThrowIfNull(precarga);
        var detalles = precarga.Alumnos.Where(a => a.TieneDetalle)
            .ToDictionary(a => $"{a.CodigoAlumno.Trim()}|{a.CodigoMateria.Trim()}", StringComparer.OrdinalIgnoreCase);
        var aplicadas = 0;
        var ausentes = 0;
        var resultado = filas.Select(f =>
        {
            if (!detalles.TryGetValue($"{f.CodigoAlumno.Trim()}|{f.CodigoMateria.Trim()}", out var d))
            {
                return f;
            }

            if (d.Ausente || d.Nota is null)
            {
                if (d.Ausente)
                {
                    ausentes++;
                }

                return f;
            }

            aplicadas++;
            var fecha = precarga.FechaExamen;
            return f.NumeroFinal switch
            {
                2 => f with { NotaFinal2 = d.Nota, FechaFinal2 = fecha ?? f.FechaFinal2 },
                3 => f with { NotaFinal3 = d.Nota, FechaFinal3 = fecha ?? f.FechaFinal3 },
                _ => f with { NotaFinal1 = d.Nota, FechaFinal1 = fecha ?? f.FechaFinal1 },
            };
        }).ToList();
        return new(resultado, aplicadas, filas.Count - aplicadas - ausentes, ausentes);
    }

    private static Dictionary<string, AlumnoCargaComisionDto> Detalles(CargaComisionDocenteDto precarga) =>
        precarga.Alumnos.Where(a => a.TieneDetalle)
            .ToDictionary(a => a.CodigoAlumno.Trim(), StringComparer.OrdinalIgnoreCase);
}
