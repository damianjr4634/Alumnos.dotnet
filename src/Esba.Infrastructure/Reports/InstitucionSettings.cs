namespace Esba.Infrastructure.Reports;

/// <summary>
/// Datos institucionales para el membrete de los reportes (sucesor del archivo
/// de plantilla "membrete_con_direccion.wmf" + FuncionesConfiguracion). Se cargan
/// del patrón Options desde appsettings (§2.3); nunca hardcodeados.
/// </summary>
public sealed class InstitucionSettings
{
    public const string SectionName = "Institucion";

    /// <summary>Nombre del instituto (fallback si la carrera no lo trae).</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Característica oficial, p.ej. "A-781".</summary>
    public string Caracteristica { get; set; } = string.Empty;

    /// <summary>Dirección postal que figura en el membrete "con dirección".</summary>
    public string Direccion { get; set; } = string.Empty;

    /// <summary>Ciudad para el membrete y el cierre de la constancia.</summary>
    public string Ciudad { get; set; } = "Buenos Aires";

    /// <summary>
    /// Ruta absoluta o relativa al logo del membrete (PNG/JPG). Si está vacía o el
    /// archivo no existe, el membrete se compone solo con texto. // TODO-asset:
    /// el original es un .wmf vectorial no usable directo en QuestPDF.
    /// </summary>
    public string? LogoPath { get; set; }
}
