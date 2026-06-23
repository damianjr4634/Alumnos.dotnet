namespace Esba.Domain.Entities;

/// <summary>
/// Tabla XXX_CONF: parámetros de configuración del sistema, clave-valor (sucesor
/// de TablaConfiguraciones.pas). PK: PARAME. El usuario solo edita el VALOR; el
/// nombre del parámetro (PARAME) y su descripción (DESCRI) los crea el sistema o
/// los SP XXX_* que los leen (ej. Web_HabilitaInscripcion, Regula_NotPromocion,
/// Mail_EnvCopia/CC/CCO), no el usuario desde la pantalla.
/// </summary>
public class ParametroConfiguracion
{
    /// <summary>PARAME VARCHAR(30): nombre del parámetro (PK).</summary>
    public required string Parame { get; set; }

    /// <summary>DESCRI VARCHAR(100): descripción legible del parámetro.</summary>
    public string? Descripcion { get; set; }

    /// <summary>VALOR VARCHAR(200): valor del parámetro (lo único editable).</summary>
    public string? Valor { get; set; }
}
