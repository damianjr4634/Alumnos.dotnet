namespace Esba.Domain.Enums;

/// <summary>
/// Tipos de constancia de alumno que emite el legacy <c>constanciaalumnos2.pas</c>
/// (botones BtnAnalitico / BtnPase / BitBtn3). El código entre paréntesis es el
/// valor TIPO que espera el SP <c>XXX_PARRAFO_CONSTANCIA</c>.
/// </summary>
public enum TipoConstancia
{
    /// <summary>'CTT' — Certificado de estudios en trámite. Valida con XXX_IMPRIME_CTT.</summary>
    CertificadoEnTramite,

    /// <summary>'PASE' — Pase en trámite. Valida con XXX_IMPRIME_PASE.</summary>
    Pase,

    /// <summary>'ANALITICO' — Certificado analítico en trámite. Valida con XXX_IMPRIME_CTT.</summary>
    Analitico,
}

/// <summary>Conversión de <see cref="TipoConstancia"/> al código TIPO del SP y al título legacy.</summary>
public static class TipoConstanciaExtensions
{
    /// <summary>Código TIPO que consume <c>XXX_PARRAFO_CONSTANCIA</c>.</summary>
    public static string ToCodigoSp(this TipoConstancia tipo) => tipo switch
    {
        TipoConstancia.CertificadoEnTramite => "CTT",
        TipoConstancia.Pase => "PASE",
        TipoConstancia.Analitico => "ANALITICO",
        _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null),
    };

    /// <summary>Título centrado del documento (literal de los handlers legacy).</summary>
    public static string ToTitulo(this TipoConstancia tipo) => tipo switch
    {
        TipoConstancia.CertificadoEnTramite => "CONSTANCIA DE CERTIFICADO DE ESTUDIOS EN TRÁMITE",
        TipoConstancia.Pase => "CONSTANCIA DE PASE EN TRÁMITE",
        TipoConstancia.Analitico => "CONSTANCIA DE CERTIFICADO ANALITICO EN TRÁMITE",
        _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null),
    };
}
