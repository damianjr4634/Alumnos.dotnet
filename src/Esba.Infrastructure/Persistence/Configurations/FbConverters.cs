using System.Globalization;
using Esba.Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Esba.Infrastructure.Persistence.Configurations;

/// <summary>
/// Conversores para las convenciones de la base legacy. Las columnas CHAR de
/// Firebird llegan con relleno de espacios, por eso toda lectura hace Trim.
/// </summary>
internal static class FbConverters
{
    /// <summary>CHAR(1) 'S'/'N' ↔ bool.</summary>
    public static readonly ValueConverter<bool, string> SiNo =
        new(v => v ? "S" : "N", v => v.Trim() == "S");

    /// <summary>CHAR(1) '*' (marcado) / NULL o vacío (desmarcado) ↔ bool, patrón de los checkboxes CTT/CA del legacy.</summary>
    public static readonly ValueConverter<bool, string?> Asterisco =
        new(v => v ? "*" : null, v => v != null && v.Trim() == "*");

    // Los datos legacy contienen CHAR(1) con espacio en blanco además de NULL
    // para "sin dato": los conversores de lectura tratan blanco como null.

    /// <summary>SEXO CHAR(1) 'F'/'M'. El valor del enum es el carácter almacenado.</summary>
    public static readonly ValueConverter<Sexo?, string?> SexoChar =
        new(
            v => v == null ? null : ((char)v).ToString(),
            v => string.IsNullOrWhiteSpace(v) ? null : (Sexo?)v.Trim()[0]);

    /// <summary>EST_CIV CHAR(1) 'S'/'C'/'D'/'V'.</summary>
    public static readonly ValueConverter<EstadoCivil?, string?> EstadoCivilChar =
        new(
            v => v == null ? null : ((char)v).ToString(),
            v => string.IsNullOrWhiteSpace(v) ? null : (EstadoCivil?)v.Trim()[0]);

    /// <summary>ESTADO CHAR(1) 'C'/'P'/'E'/'N'.</summary>
    public static readonly ValueConverter<EstadoAlumno?, string?> EstadoAlumnoChar =
        new(
            v => v == null ? null : ((char)v).ToString(),
            v => string.IsNullOrWhiteSpace(v) ? null : (EstadoAlumno?)v.Trim()[0]);

    /// <summary>DNI CHAR(1) dígito '0'..'3' (índice del combo legacy). Valores no numéricos se leen como null.</summary>
    public static readonly ValueConverter<SituacionDni?, string?> SituacionDniChar =
        new(
            v => v == null ? null : ((int)v).ToString(CultureInfo.InvariantCulture),
            v => v == null || v.Trim().Length == 0 || !char.IsAsciiDigit(v.Trim()[0])
                ? null
                : (SituacionDni?)(v.Trim()[0] - '0'));
}
