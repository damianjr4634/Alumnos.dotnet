namespace Esba.Domain.Examenes;

/// <summary>
/// Textos fijos de la citación a profesores (Imp_Mesas_citacion_enc / _pie de
/// Impresiones.pas), con las erratas del original corregidas ("Superiodidad",
/// "dirigime", "debera").
/// </summary>
public static class CitacionDocentesTextos
{
    public const string Titulo = "HORARIO DE EVALUACIONES - EXAMENES PARA PROFESORES";

    public const string Comunicacion =
        "Tengo el agrado de dirigirme a Ud. comunicándole la fecha de reunión de las mesas de " +
        "evaluación / examen que deberá integrar:";

    public const string Notificado = "NOTIFICADO:.......................................";

    public const string FechaEnBlanco = "FECHA: ___/___/_____";

    public const string Saludo = "Saluda a Ud. atentamente:";

    public const string Reglamento =
        "Es obligación de los señores profesores asistir puntualmente a las/los evaluaciones/exámenes a que " +
        "sean convocados por la Superioridad, entendiéndose, además, que toda inasistencia no justificada será " +
        "considerada doble (Art. 53. del Reglamento General). Cualquier objeción al presente horario, deberá " +
        "formularse dentro de los tres días de recibido; en caso contrario, se tendrá por definitivamente " +
        "aceptado. Si el Señor profesor desempeña tareas en otro establecimiento, se servirá presentar al mismo " +
        "esta comunicación a sus efectos.";

    public const string Original = "O R I G I N A L";

    public const string Duplicado = "D U P L I C A D O";
}
