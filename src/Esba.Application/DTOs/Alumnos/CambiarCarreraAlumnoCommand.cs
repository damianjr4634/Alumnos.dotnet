namespace Esba.Application.DTOs.Alumnos;

/// <summary>
/// Copiar o mover un alumno a otra carrera (sucesor de dxBarButton29Click /
/// dxBarButton30Click de FrmEsba.pas: "Copiar alumno a la carrera…" y "Mover alumno a
/// la carrera…"). El usuario llega por claims, no por el global CodUsu.
/// </summary>
public sealed record CambiarCarreraAlumnoCommand
{
    public required string CodigoAlumno { get; init; }

    public required string CodigoCarreraOrigen { get; init; }

    public required string CodigoCarreraDestino { get; init; }

    public required int CodigoUsuario { get; init; }
}
