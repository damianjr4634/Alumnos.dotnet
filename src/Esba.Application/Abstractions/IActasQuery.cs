using Esba.Application.DTOs.Examenes;

namespace Esba.Application.Abstractions;

/// <summary>
/// Lecturas para las actas de examen (sucesoras de los SqlComi/SqlDatos de
/// lstactasARegular.pas, lstactasreincorporacion.pas, lstactasexamenes.pas y
/// lstactasMesas.pas). Todo SQL parametrizado (§1.3); sin staging ni globales.
/// </summary>
public interface IActasQuery
{
    /// <summary>
    /// Cabeceras (comisión-materia) de COMARM para un acta por comisión.
    /// <paramref name="cuatrimestreAnio"/> se normaliza al formato de columna CHAR(3)
    /// "124" (sin barra). <paramref name="filtrarPorCondicion"/> aplica el
    /// <c>EXISTS</c> sobre <paramref name="condiciones"/> (A/REGULAR y REINCORPORA).
    /// </summary>
    Task<IReadOnlyList<ActaComisionCabeceraDto>> ObtenerCabecerasComisionAsync(
        string codigoCarrera,
        string cuatrimestreAnio,
        short? cutuco,
        string? codigoMateria,
        IReadOnlyList<string> condiciones,
        bool filtrarPorCondicion,
        CancellationToken ct);

    /// <summary>
    /// Alumnos (CURSADA ⨝ ALUMNOS) que cumplen <paramref name="condiciones"/> para el
    /// mismo filtro de carrera/cuatrimestre/comisión/materia. Se agrupan por comisión
    /// en el handler.
    /// </summary>
    Task<IReadOnlyList<ActaAlumnoDto>> ObtenerAlumnosComisionAsync(
        string codigoCarrera,
        string cuatrimestreAnio,
        short? cutuco,
        string? codigoMateria,
        IReadOnlyList<string> condiciones,
        CancellationToken ct);

    /// <summary>Cabecera del acta volante de una mesa (MESAS ⨝ DOCENTES ⨝ MATERIAS). null si no existe.</summary>
    Task<ActaMesaCabeceraDto?> ObtenerCabeceraMesaAsync(int mesa, string codigoCarrera, CancellationToken ct);

    /// <summary>Candidatos de la mesa vía el SP XXX_MESAS_ALUMNOS (PERM_EXA, COD_ALU, apellido, nombre).</summary>
    Task<IReadOnlyList<ActaAlumnoDto>> ObtenerAlumnosMesaAsync(
        int mesa, string codigoCarrera, string tipoExamen, CancellationToken ct);
}
