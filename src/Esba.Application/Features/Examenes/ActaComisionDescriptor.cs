using Esba.Domain.Examenes;

namespace Esba.Application.Features.Examenes;

/// <summary>
/// Metadatos de cada <see cref="TipoActaComision"/>: las condiciones de
/// <c>CURSADA.CONDICION</c> que la variante lista, si la cabecera (COMARM) se filtra
/// con un <c>EXISTS</c> sobre esas condiciones, el título impreso y si el cuerpo
/// incluye la línea "Correspondiente al N° CUATRIMESTRE de estudios".
/// Centraliza lo que en el legacy estaba repetido entre lstactasARegular.pas,
/// lstactasreincorporacion.pas y lstactasexamenes.pas.
/// </summary>
public sealed record ActaComisionDescriptor
{
    public required IReadOnlyList<string> Condiciones { get; init; }

    /// <summary>
    /// A/REGULAR y REINCORPORA filtran las comisiones de COMARM con
    /// <c>EXISTS(... CURSADA con la condición)</c>; la variante de exámenes
    /// (CURSANDO/RECURSANDO) lista todas las comisiones del cuatrimestre.
    /// </summary>
    public required bool FiltrarCabeceraPorCondicion { get; init; }

    public required string Titulo { get; init; }

    /// <summary>Solo A/REGULAR y REINCORPORA imprimen la línea del cuatrimestre.</summary>
    public required bool MuestraCorrespondienteCuatrimestre { get; init; }

    public static ActaComisionDescriptor Para(TipoActaComision tipo) => tipo switch
    {
        TipoActaComision.ARegular => new ActaComisionDescriptor
        {
            Condiciones = ["A/REGULAR"],
            FiltrarCabeceraPorCondicion = true,
            Titulo = "ACTA DE EXAMENES DE ALUMNOS A/REGULAR",
            MuestraCorrespondienteCuatrimestre = true,
        },
        TipoActaComision.Reincorporacion => new ActaComisionDescriptor
        {
            Condiciones = ["REINCORPORA"],
            FiltrarCabeceraPorCondicion = true,
            Titulo = "ACTA DE EXAMENES DE ALUMNOS A REINCORPORAR",
            MuestraCorrespondienteCuatrimestre = true,
        },
        TipoActaComision.Examenes => new ActaComisionDescriptor
        {
            Condiciones = ["CURSANDO", "RECURSANDO"],
            FiltrarCabeceraPorCondicion = false,
            Titulo = "ACTA DE EXAMENES",
            MuestraCorrespondienteCuatrimestre = false,
        },
        _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Tipo de acta por comisión desconocido."),
    };
}
