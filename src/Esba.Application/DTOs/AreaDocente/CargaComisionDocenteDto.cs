using Esba.Domain.Enums;

namespace Esba.Application.DTOs.AreaDocente;

/// <summary>
/// Pantalla de precarga de una comisión: datos de la comisión (COMARM + carrera +
/// materia + titular), la cabecera de la carga si existe y una fila por alumno
/// cursando/recursando con los valores actuales de CURSADA y los precargados.
/// Qué campos se muestran lo decide <c>EsquemaCargaCursado.Para(CodigoCarrera, TipoCarrera)</c>.
/// </summary>
public sealed record CargaComisionDocenteDto
{
    public required string CodigoCarrera { get; init; }

    public string? NombreCarrera { get; init; }

    /// <summary>CARRERA.TIPO ('TER'/'BAC'/'BAD'): junto con el código decide la variante de regularización.</summary>
    public string? TipoCarrera { get; init; }

    public short Cutuco { get; init; }

    public required string CodigoMateria { get; init; }

    public string? SiglaMateria { get; init; }

    public string? NombreMateria { get; init; }

    public required string CuatrimestreAnio { get; init; }

    /// <summary>COMARM.CODPROFES: titular actual de la comisión. Solo él (o secretaría) carga.</summary>
    public string? CodigoDocenteTitular { get; init; }

    public string? NombreDocenteTitular { get; init; }

    /// <summary>DOC_CARGA_COMISION.ID; null = todavía no hay carga.</summary>
    public int? CargaId { get; init; }

    public EstadoCargaDocente? Estado { get; init; }

    public string? ObservacionesCarga { get; init; }

    public DateTime? FechaModificacion { get; init; }

    public DateTime? FechaFinalizacion { get; init; }

    public DateTime? FechaEfectivizacion { get; init; }

    public IReadOnlyList<AlumnoCargaComisionDto> Alumnos { get; init; } = [];

    public bool TieneCarga => CargaId is not null;
}

/// <summary>
/// Un alumno de la comisión. Los campos "Cursada*" son lo que hoy dice CURSADA (lo
/// que secretaría ya tiene cargado); los sin prefijo son la precarga del docente
/// (null si no hay detalle). La UI precarga los primeros cuando no hay detalle.
/// </summary>
public sealed record AlumnoCargaComisionDto
{
    public required string CodigoAlumno { get; init; }

    public string? Apellido { get; init; }

    public string? Nombre { get; init; }

    public string? Condicion { get; init; }

    public int CursadaIndice { get; init; }

    public decimal? CursadaEvaluacion1 { get; init; }

    public decimal? CursadaRecuperatorio { get; init; }

    public decimal? CursadaEvaluacion2 { get; init; }

    public decimal? CursadaEvaluacion3 { get; init; }

    public decimal? CursadaNotaRegular { get; init; }

    public decimal? CursadaNotaFinal { get; init; }

    public short? CursadaTotalHoras { get; init; }

    public short? CursadaInasistencias { get; init; }

    public short? CursadaJustificadas { get; init; }

    /// <summary>DOC_CARGA_COMISION_DET.ID; null = el docente todavía no cargó nada para este alumno.</summary>
    public int? DetalleId { get; init; }

    public decimal? Evaluacion1 { get; init; }

    public decimal? Recuperatorio { get; init; }

    public decimal? Evaluacion2 { get; init; }

    public decimal? Evaluacion3 { get; init; }

    public decimal? NotaRegular { get; init; }

    public decimal? NotaFinal { get; init; }

    public short? TotalHoras { get; init; }

    public short? Inasistencias { get; init; }

    public short? Justificadas { get; init; }

    public string? Observaciones { get; init; }

    public bool TieneDetalle => DetalleId is not null;
}
