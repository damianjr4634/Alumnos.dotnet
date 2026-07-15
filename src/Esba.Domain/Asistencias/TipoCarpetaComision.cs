namespace Esba.Domain.Asistencias;

/// <summary>
/// Variantes de "carpeta" impresa por comisión del legacy, que comparten la nómina
/// (COMARM + CURSADA + ALUMNOS cursando/recursando) y difieren solo en la grilla
/// que el docente completa a mano:
/// <list type="bullet">
/// <item><see cref="Asistencia"/> — lstplanasis.pas ("Carpeta asistencia": 25 días + INA/ANT/TOT).</item>
/// <item><see cref="TrabajosPracticos"/> — lstNotasyPractico.pas con plantilla trabajos_practicos.wmf
/// ("Carpeta de trabajos practicos": TP 1–5 con fecha + condición).</item>
/// </list>
/// </summary>
public enum TipoCarpetaComision
{
    /// <summary>Planilla de asistencia en blanco (días + inasistencias).</summary>
    Asistencia = 0,

    /// <summary>Planilla de trabajos prácticos (TP 1–5 con fecha + condición).</summary>
    TrabajosPracticos = 1,
}
