namespace Esba.Application.DTOs.Examenes;

/// <summary>
/// Alta de un permiso de examen (sucesor del INSERT de
/// PermisoExamen.GrabaPermisoClick). El INDICE lo genera el trigger PERMEXA_BI0;
/// FECH_EMI es la fecha de emisión (hoy).
/// </summary>
public sealed record CrearPermisoExamenCommand
{
    public required string CodigoCarrera { get; init; }

    public required string CodigoAlumno { get; init; }

    public string? Apellido { get; init; }

    /// <summary>PERM_EXA: número de permiso (opcional).</summary>
    public int? NumeroPermiso { get; init; }

    public required int Mesa { get; init; }

    public required int Cutuco { get; init; }

    public required string CodigoMateria { get; init; }

    public required int CodigoUsuario { get; init; }
}
