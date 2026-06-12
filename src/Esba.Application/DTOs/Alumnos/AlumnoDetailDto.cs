using Esba.Domain.Enums;

namespace Esba.Application.DTOs.Alumnos;

/// <summary>
/// Ficha completa del alumno (columnas del SELECT de detalle que FrmEsba usa
/// para poblar FrmAltaAlumno en modo edición). Excluye las columnas sin uso en
/// pantallas (MOROSO*, PN, CD, RESIDE, CR) y las del módulo administrativo.
/// </summary>
public sealed record AlumnoDetailDto
{
    public required string Codigo { get; init; }

    public required string CodigoCarrera { get; init; }

    public string? Matriz { get; init; }

    public string? Apellido { get; init; }

    public string? Nombre { get; init; }

    public string? DocumentoExpedidoPor { get; init; }

    public Sexo? Sexo { get; init; }

    public string? Nacionalidad { get; init; }

    public EstadoCivil? EstadoCivil { get; init; }

    public DateOnly? FechaNacimiento { get; init; }

    public string? LugarNacimiento { get; init; }

    public string? ProvinciaNacimiento { get; init; }

    public string? Domicilio { get; init; }

    public string? Localidad { get; init; }

    public short? CodigoPostal { get; init; }

    public string? CaracteristicaTelefono { get; init; }

    public string? Telefono { get; init; }

    public string? Celular { get; init; }

    public string? Mail { get; init; }

    public DateOnly? FechaIngreso { get; init; }

    public string? ColegioPrimario { get; init; }

    public string? AnioPrimario { get; init; }

    public string? TituloPrimario { get; init; }

    public string? ColegioSecundario { get; init; }

    public string? AnioSecundario { get; init; }

    public string? TituloSecundario { get; init; }

    public string? InstitucionTerciaria { get; init; }

    public string? AnioTerciario { get; init; }

    public string? TituloTerciario { get; init; }

    public string? Empresa { get; init; }

    public string? Rubro { get; init; }

    public string? Cargo { get; init; }

    public string? Antiguedad { get; init; }

    public string? DomicilioLaboral { get; init; }

    public string? TelefonoLaboral { get; init; }

    public string? InternoLaboral { get; init; }

    public bool TieneCertificadoEnTramite { get; init; }

    public DateOnly? FechaCertificadoEnTramite { get; init; }

    public SituacionDni? SituacionDni { get; init; }

    public bool PresentoCa { get; init; }

    public bool Baja { get; init; }

    public string? Observaciones { get; init; }

    public bool? PresentoFoto { get; init; }

    public bool? PresentoAptoFisico { get; init; }

    public DateOnly? FechaAptoFisico { get; init; }

    public string? UsuarioWeb { get; init; }

    public byte[]? Foto { get; init; }

    public bool? PresentoPartidaNacimiento { get; init; }

    public bool? PresentoLibreta { get; init; }

    public DateOnly? FechaLibreta { get; init; }

    public EstadoAlumno? Estado { get; init; }

    public Genero? Genero { get; init; }

    public bool? NominaPase { get; init; }
}
