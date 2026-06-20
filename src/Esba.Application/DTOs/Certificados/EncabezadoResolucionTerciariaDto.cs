namespace Esba.Application.DTOs.Certificados;

/// <summary>
/// Encabezado de la resolución de equivalencia terciaria (sucesor del primer SELECT de
/// <c>BtnImprimirFormatoNuevoClick</c> de lst_impresion_equivalencia_terc.pas): datos del
/// alumno y la lista de actas internas involucradas. null si el alumno no existe.
/// </summary>
public sealed record EncabezadoResolucionTerciariaDto
{
    /// <summary>EXTRACT(YEAR FROM CURRENT_DATE): año de matrícula citado en el VISTO.</summary>
    public int AnioActual { get; init; }

    /// <summary>COD_ALU con "DNI" separado ("DNI 12345678").</summary>
    public string? CodigoAlumno { get; init; }

    /// <summary>APELLIDO + NOM_APE.</summary>
    public string? NombreAlumno { get; init; }

    /// <summary>LIST de las actas internas distintas (ya formateadas "número/AA") de las equivalencias del alumno.</summary>
    public string? ActasInternas { get; init; }
}
