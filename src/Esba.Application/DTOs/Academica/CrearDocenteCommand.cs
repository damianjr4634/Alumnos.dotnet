namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Alta de docente (sucesor del INSERT de FrmAltaModProfes.GrabarClick). El
/// código (CODPROFES) lo ingresa el usuario. La baja no se fija acá (tiene su
/// propio flujo). Alcance esencial (hito 10.2).
/// </summary>
public sealed record CrearDocenteCommand
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

    public bool EnLicencia { get; init; }

    public DateOnly? FechaLicencia { get; init; }
}
