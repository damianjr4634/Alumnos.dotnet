namespace Esba.Domain.Examenes;

/// <summary>
/// Variantes de acta de examen "por comisión" del legacy, que comparten estructura
/// (COMARM + CURSADA + ALUMNOS) y difieren en la condición de la cursada que listan
/// y en el título impreso:
/// <list type="bullet">
/// <item><see cref="ARegular"/> — lstactasARegular.pas (condición A/REGULAR).</item>
/// <item><see cref="Reincorporacion"/> — lstactasreincorporacion.pas (condición REINCORPORA).</item>
/// <item><see cref="Examenes"/> — lstactasexamenes.pas (condiciones CURSANDO/RECURSANDO).</item>
/// </list>
/// </summary>
public enum TipoActaComision
{
    /// <summary>Acta de exámenes de alumnos A/REGULAR.</summary>
    ARegular = 0,

    /// <summary>Acta de exámenes de alumnos a reincorporar.</summary>
    Reincorporacion = 1,

    /// <summary>Acta de exámenes de alumnos cursando/recursando.</summary>
    Examenes = 2,
}
