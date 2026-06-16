namespace Esba.Application.Abstractions;

/// <summary>
/// Wrapper de <c>XXX_PARRAFO_CONSTANCIA</c>: arma el párrafo de cuerpo de la
/// constancia (incluye nombre, documento y carrera del alumno) según el tipo.
/// </summary>
public interface IParrafoConstanciaProcedure
{
    /// <summary>
    /// <paramref name="tipo"/> es el código TIPO del SP ('CTT', 'PASE', 'ANALITICO',
    /// 'CE-xx'). Devuelve el texto del párrafo (FPARRAFO), o cadena vacía si el SP
    /// no produjo texto.
    /// </summary>
    Task<string> ObtenerAsync(string codigoAlumno, string codigoCarrera, string tipo, CancellationToken ct);
}
