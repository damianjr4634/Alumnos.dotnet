namespace Esba.Domain.Examenes;

/// <summary>
/// Autoridad que firma la citación a profesores (combo "Firma" del panel de parámetros
/// de dxBarButton20Click en FrmEsba.pas: "Rector/a,Secretaria,Dir.Estud." con valores
/// R/S/D). El nombre sale de CARRERA (RECTOR/SECRETARIA/DIRESTU); el cargo impreso es
/// el rótulo fijo del legacy (Imp_Mesas_citacion_pie).
/// </summary>
public enum FirmanteCitacion
{
    Rector,
    Secretaria,
    DirectorEstudios,
}

public static class FirmanteCitacionExtensions
{
    /// <summary>Rótulo del cargo bajo la firma, como lo imprimía Imp_Mesas_citacion_pie.</summary>
    public static string Cargo(this FirmanteCitacion firmante) => firmante switch
    {
        FirmanteCitacion.Rector => "RECTOR/A",
        FirmanteCitacion.Secretaria => "SECRETARIA",
        FirmanteCitacion.DirectorEstudios => "DIR. DE ESTUDIOS",
        _ => string.Empty,
    };
}
