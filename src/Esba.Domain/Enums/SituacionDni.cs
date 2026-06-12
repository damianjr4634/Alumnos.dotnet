namespace Esba.Domain.Enums;

/// <summary>
/// ALUMNOS.DNI CHAR(1): índice del combo "dni" de FrmAltaAlumno guardado como
/// dígito ('0'..'3'). Indica la actualización del documento presentada
/// ("8 años", "16 años", "Mayor de edad", "Sin Documento").
/// </summary>
public enum SituacionDni
{
    ActualizadoOchoAnios = 0,
    ActualizadoDieciseisAnios = 1,
    MayorDeEdad = 2,
    SinDocumento = 3,
}
