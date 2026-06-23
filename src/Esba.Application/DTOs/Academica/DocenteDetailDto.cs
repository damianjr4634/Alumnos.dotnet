namespace Esba.Application.DTOs.Academica;

/// <summary>Docente completo (alcance esencial) para precargar el formulario de edición.</summary>
public sealed record DocenteDetailDto
{
    public required string Codigo { get; init; }

    public string? Nombre { get; init; }

    public string? TipoDocumento { get; init; }

    public string? NumeroDocumento { get; init; }

    public DateOnly? FechaNacimiento { get; init; }

    public string? Direccion { get; init; }

    public string? Piso { get; init; }

    public string? Departamento { get; init; }

    public string? CodigoPostal { get; init; }

    public string? Localidad { get; init; }

    public string? TelefonoParticular { get; init; }

    public string? TelefonoMensajes { get; init; }

    public string? Interno { get; init; }

    public DateOnly? FechaIngreso { get; init; }

    public DateOnly? FechaBaja { get; init; }

    public bool EnLicencia { get; init; }

    public DateOnly? FechaLicencia { get; init; }
}
