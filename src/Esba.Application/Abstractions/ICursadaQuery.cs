using Esba.Application.DTOs.Academica;

namespace Esba.Application.Abstractions;

/// <summary>Lecturas de la cursada de un alumno (grilla de InscripcionDeMaterias.pas).</summary>
public interface ICursadaQuery
{
    Task<IReadOnlyList<CursadaListItemDto>> ListarPorAlumnoAsync(string codigoCarrera, string codigoAlumno, CancellationToken ct);
}
