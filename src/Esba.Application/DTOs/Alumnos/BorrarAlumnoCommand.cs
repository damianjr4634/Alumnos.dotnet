namespace Esba.Application.DTOs.Alumnos;

/// <summary>
/// Eliminación física de un alumno y todo su historial (sucesor de
/// dxBarButton43Click de FrmEsba.pas → XXX_BORRA_ALUMNO). Solo supervisores: el
/// flag viene del claim y se verifica en el handler además de en el SP (§2.7: el
/// servidor deniega, la UI solo oculta).
/// </summary>
public sealed record BorrarAlumnoCommand
{
    public required string CodigoAlumno { get; init; }

    public required string CodigoCarrera { get; init; }

    public required int CodigoUsuario { get; init; }

    public required bool EsSupervisor { get; init; }
}
