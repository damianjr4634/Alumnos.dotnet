namespace Esba.Application.DTOs.Academica;

/// <summary>Campos comunes de comisión (alta y modificación) para compartir validación.</summary>
public interface IComisionCampos
{
    string CodigoCarrera { get; }

    short Cutuco { get; }

    string CodigoMateria { get; }

    string CuatrimestreAnio { get; }

    IReadOnlyList<HorarioDiaComision> Horario { get; }
}
