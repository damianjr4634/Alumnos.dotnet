namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Eliminación de la cursada de una materia (sucesor del DELETE de
/// InscripcionDeMaterias.eliminacursadaClick). La confirmación previa es de la
/// UI; el trigger CURSADA_BI0 limpia RECURSA si la condición era RECURSANDO.
/// </summary>
public sealed record EliminarInscripcionCommand
{
    public required string CodigoCarrera { get; init; }

    public required string CodigoAlumno { get; init; }

    public required string CodigoMateria { get; init; }
}
