namespace Esba.Domain.Enums;

/// <summary>
/// ALUMNOS.ESTADO CHAR(1): mapeo explícito de FrmAltaAlumno.CbEstado
/// ('C' cursando, 'P' pase, 'E' egresado, 'N' no se sabe).
/// El valor del enum es el carácter almacenado en la base.
/// </summary>
public enum EstadoAlumno
{
    Cursando = 'C',
    Pase = 'P',
    Egresado = 'E',
    NoSeSabe = 'N',
}
