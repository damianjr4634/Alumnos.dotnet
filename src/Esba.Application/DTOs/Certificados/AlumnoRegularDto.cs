namespace Esba.Application.DTOs.Certificados;

/// <summary>
/// Cursada vigente del alumno que respalda la Constancia de Alumno Regular (sucesor
/// del SELECT de FormShow de constanciaalumnoregular.pas sobre CURSADA + CARRERA +
/// ALUMNOS). Null en la query cuando el alumno no está CURSANDO/RECURSANDO en el
/// cuatrimestre vigente.
/// </summary>
public sealed record AlumnoRegularDto
{
    /// <summary>Apellido y nombre del alumno.</summary>
    public required string NombreCompleto { get; init; }

    /// <summary>CUTUCO: código de comisión/turno (1er dígito = cuatrimestre, 2do = turno).</summary>
    public int Cutuco { get; init; }

    /// <summary>DISTANCIA = 'S': modalidad a distancia.</summary>
    public bool EsADistancia { get; init; }

    /// <summary>DICTAMEN del Consejo Federal (solo se imprime en modalidad a distancia).</summary>
    public string? Dictamen { get; init; }

    /// <summary>MAIL del alumno (para el envío por correo del hito 10.4b).</summary>
    public string? Mail { get; init; }
}
