namespace Esba.Web.Seguridad;

/// <summary>
/// Políticas de autorización. Por ahora solo "Supervisores", que protege las
/// pantallas de Administración (sucesor del flag SUPERV). La autorización fina
/// por área funcional (mapa de BARRA_OPC / BARRA_SEGU) es del hito 12.
/// </summary>
public static class EsbaPolicies
{
    public const string Supervisores = "Supervisores";
}
