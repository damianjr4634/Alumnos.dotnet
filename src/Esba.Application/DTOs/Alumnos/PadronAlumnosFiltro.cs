namespace Esba.Application.DTOs.Alumnos;

/// <summary>
/// Filtros del padrón de alumnos. Cada propiedad replica un control del
/// buscador global de FrmEsba (legacy):
/// - <see cref="Texto"/>: TxtBusqueda — "apellido" o "apellido:nombre"; también
///   matchea contra mail y código de alumno. El atajo legacy "_CARRE" de ese
///   control se traduce acá en <see cref="CodigoCarrera"/> (lo parsea la UI).
/// - <see cref="CodigoAlumno"/>: parámetro ACodAlu (búsqueda exacta al volver
///   de otra pantalla).
/// - <see cref="BuscarDadosDeBaja"/>: checkbox chbBuscarBajas — el padrón
///   muestra solo activos o solo bajas, nunca mezclados.
/// - <see cref="IncluirCarrerasEnDesuso"/>: checkbox cbCarreDesuso.
/// - <see cref="CodigoUsuario"/>/<see cref="EsSupervisor"/>: reemplazan las
///   globals CodUsu/Superv de FuncionesConfiguracion; vienen de los claims del
///   usuario. Si no es supervisor, solo ve las carreras habilitadas en BARRA_SEGU.
/// </summary>
public sealed record PadronAlumnosFiltro
{
    public string? Texto { get; init; }

    public string? CodigoCarrera { get; init; }

    public string? CodigoAlumno { get; init; }

    public bool BuscarDadosDeBaja { get; init; }

    public bool IncluirCarrerasEnDesuso { get; init; }

    public int? CodigoUsuario { get; init; }

    public bool EsSupervisor { get; init; }

    public int Skip { get; init; }

    public int Take { get; init; } = 50;
}
