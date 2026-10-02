using Esba.Application.DTOs.Academica;

namespace Esba.Application.Abstractions;

/// <summary>Lecturas de la cursada de un alumno (grilla de InscripcionDeMaterias.pas).</summary>
public interface ICursadaQuery
{
    Task<IReadOnlyList<CursadaListItemDto>> ListarPorAlumnoAsync(string codigoCarrera, string codigoAlumno, CancellationToken ct);

    /// <summary>
    /// Códigos de materia que el alumno ya tiene registrados en la carrera, en CURSADA o en
    /// ANALITIC. Es el mismo criterio con que XXX_INSC_VALMAT (TIPO 'A') rechaza una
    /// equivalencia; sirve para no ofrecer esas materias en el combo.
    /// </summary>
    Task<IReadOnlySet<string>> ListarMateriasRegistradasAsync(string codigoCarrera, string codigoAlumno, CancellationToken ct);
}
