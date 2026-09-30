namespace Esba.Application.DTOs.Examenes;

/// <summary>
/// Docente que integra alguna mesa del rango (SqlDatos de Imp_Mesas_citacion:
/// DISTINCT DOCENTE, CODPROFES de MESAS ⨝ DOCENTES por titular/vocal1/vocal2).
/// </summary>
public sealed record DocenteCitadoDto
{
    public required string CodigoProfesor { get; init; }

    public string? Docente { get; init; }
}

/// <summary>
/// Una mesa a la que un docente está convocado (SqlDatos2 de Imp_Mesas_citacion). Hay
/// una fila por (docente, mesa): la misma mesa aparece para el titular y cada vocal.
/// </summary>
public sealed record MesaCitacionDto
{
    public required string CodigoProfesor { get; init; }

    public DateOnly? FechaExamen { get; init; }

    /// <summary>MESAS.HORA NUMERIC(4,0) en formato HHMM; se formatea con <c>FormatoMesa.Hora</c>.</summary>
    public int? Hora { get; init; }

    /// <summary>Sigla de la materia o, si no tiene, su descripción.</summary>
    public string? Materia { get; init; }

    public int Mesa { get; init; }

    public int? Aula { get; init; }

    public required string CodigoCarrera { get; init; }
}

/// <summary>
/// Una mesa del parte diario (SqlDatos de Imp_Mesas_ParteDiario): tribunal, comisiones
/// y cantidad de alumnos con permiso (PERMEXA) para esa mesa y fecha.
/// </summary>
public sealed record ParteDiarioMesaDto
{
    public DateOnly? FechaExamen { get; init; }

    public required string CodigoCarrera { get; init; }

    public string? NombreCarrera { get; init; }

    public int Mesa { get; init; }

    public int? Hora { get; init; }

    public string? Materia { get; init; }

    public string? Titular { get; init; }

    public string? Vocal1 { get; init; }

    public string? Vocal2 { get; init; }

    public int? Comision1 { get; init; }

    public int? Comision2 { get; init; }

    public int? Comision3 { get; init; }

    /// <summary>Alumnos con permiso de examen para la mesa en esa fecha (COUNT sobre PERMEXA).</summary>
    public int CantidadAlumnos { get; init; }

    public int? Aula { get; init; }
}

/// <summary>Autoridades de una carrera que pueden firmar la citación (CARRERA.RECTOR/SECRETARIA/DIRESTU).</summary>
public sealed record AutoridadesCarreraDto
{
    public string? Rector { get; init; }

    public string? Secretaria { get; init; }

    public string? DirectorEstudios { get; init; }
}
