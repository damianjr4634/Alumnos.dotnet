namespace Esba.Domain.Enums;

/// <summary>
/// DOC_CARGA_COMISION.ESTADO / DOC_CARGA_MESA.ESTADO (CHAR(3), migración 2026-10-03):
/// ciclo de vida de una precarga del docente (hito 19). Vive en la cabecera (una por
/// comisión / por mesa), no por alumno.
/// </summary>
public enum EstadoCargaDocente
{
    /// <summary>'BOR': borrador, el docente titular edita.</summary>
    Borrador = 0,

    /// <summary>'FIN': finalizada por el docente; sigue viendo la comisión pero no toca nada.</summary>
    Finalizada = 1,

    /// <summary>'EFE': efectivizada por secretaría sobre las tablas reales.</summary>
    Efectivizada = 2,
}

/// <summary>Códigos de 3 letras que persiste la columna ESTADO. Único lugar que conoce la correspondencia.</summary>
public static class EstadoCargaDocenteCodigo
{
    public const string Borrador = "BOR";
    public const string Finalizada = "FIN";
    public const string Efectivizada = "EFE";

    public static string ACodigo(EstadoCargaDocente estado) => estado switch
    {
        EstadoCargaDocente.Borrador => Borrador,
        EstadoCargaDocente.Finalizada => Finalizada,
        EstadoCargaDocente.Efectivizada => Efectivizada,
        _ => throw new ArgumentOutOfRangeException(nameof(estado), estado, "Estado de carga desconocido."),
    };

    /// <summary>Un código no reconocido es un dato corrupto: se falla fuerte (la columna tiene CHECK).</summary>
    public static EstadoCargaDocente DesdeCodigo(string? codigo) => codigo?.Trim().ToUpperInvariant() switch
    {
        Borrador => EstadoCargaDocente.Borrador,
        Finalizada => EstadoCargaDocente.Finalizada,
        Efectivizada => EstadoCargaDocente.Efectivizada,
        _ => throw new InvalidOperationException($"Estado de carga docente con valor desconocido '{codigo}'."),
    };

    /// <summary>Null = no hay carga todavía.</summary>
    public static EstadoCargaDocente? DesdeCodigoOpcional(string? codigo) =>
        string.IsNullOrWhiteSpace(codigo) ? null : DesdeCodigo(codigo);

    public static string Etiqueta(EstadoCargaDocente? estado) => estado switch
    {
        EstadoCargaDocente.Borrador => "En borrador",
        EstadoCargaDocente.Finalizada => "Finalizada",
        EstadoCargaDocente.Efectivizada => "Efectivizada",
        _ => "Sin cargar",
    };
}
