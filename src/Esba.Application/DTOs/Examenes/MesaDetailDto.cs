namespace Esba.Application.DTOs.Examenes;

/// <summary>Mesa completa para precargar el formulario de edición.</summary>
public sealed record MesaDetailDto
{
    public required string CodigoCarrera { get; init; }

    public required int NumeroMesa { get; init; }

    public string? CodigoMateria { get; init; }

    public int Llamado { get; init; }

    public DateOnly FechaExamen { get; init; }

    public int Hora { get; init; }

    public string? Titular { get; init; }

    public string? Vocal1 { get; init; }

    public string? Vocal2 { get; init; }

    public int Aula { get; init; }

    public int Comision1 { get; init; }

    public int Comision2 { get; init; }

    public int Comision3 { get; init; }

    public string? CodigoTipo { get; init; }
}
