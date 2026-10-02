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

    /// <summary>
    /// Ruta (absoluta o relativa al directorio de ejecución) al papel membretado JPG de
    /// la resolución de equivalencia terciaria (sucesor de "membrete_con_direccion.jpg").
    /// Si está vacía o no existe, la resolución se imprime sin fondo (para preimpreso).
    /// </summary>
    public string? MembreteResolucionPath { get; set; }

    /// <summary>
    /// Ruta (absoluta o relativa al directorio de ejecución) al papel membretado JPG de la
    /// Constancia de Alumno Regular. Es el mismo "membrete_con_direccion.jpg" (A4) que usaba
    /// constanciaalumnoregular.pas. Si está vacía o no existe, la constancia se imprime sin fondo.
    /// </summary>
    public string? MembreteConstanciaRegularPath { get; set; }

    /// <summary>
    /// Ruta (absoluta o relativa al directorio de ejecución) al papel membretado JPG (A4) de
    /// las demás constancias de impresión: constancia de alumno, materias aprobadas, examen
    /// final y equivalencia bachiller. Es el mismo "membrete_con_direccion.jpg". Si está vacía
    /// o no existe, se imprimen sin fondo (para papel preimpreso).
    /// </summary>
    public string? MembreteConstanciaPath { get; set; }

    /// <summary>
    /// Papel membretado JPG en hoja **Oficio** ("membrete_con_direccion_oficio.jpg"), para los
    /// reportes en ese tamaño que lleven membrete. Si está vacía o no existe, salen sin fondo.
    /// </summary>
    public string? MembreteOficioPath { get; set; }

    /// <summary>
    /// Sello institucional (sucesor de CARPETA_FIRMAS\sello.jpg). Va en la última hoja de toda
    /// impresión con membrete, al lado de las firmas de las autoridades. Sin archivo se imprime
    /// el rótulo "SELLO".
    /// </summary>
    public string? SelloPath { get; set; }

    /// <summary>
    /// Imagen de la firma de la rectora (sucesor de CARPETA_FIRMAS\firma_recto.jpg). Sale sobre
    /// su nombre en toda impresión que lo incluya; sin archivo queda el espacio en blanco.
    /// </summary>
    public string? FirmaRectorPath { get; set; }

    /// <summary>Imagen de la firma de la secretaria (sucesor de firma_secre.jpg), misma regla.</summary>
    public string? FirmaSecretariaPath { get; set; }

    /// <summary>Imagen de la firma del director/a de estudios (sucesor de firma_direc.jpg), misma regla.</summary>
    public string? FirmaDirectorEstudiosPath { get; set; }
}
