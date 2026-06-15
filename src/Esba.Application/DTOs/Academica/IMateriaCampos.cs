namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Campos editables de una materia, comunes al alta y a la modificación
/// (sucesor del formulario único de altamodifmaterias.pas). Permite compartir
/// las reglas de validación entre ambos comandos sin duplicarlas.
/// </summary>
public interface IMateriaCampos
{
    string CodigoCarrera { get; }

    string Codigo { get; }

    string? Nombre { get; }

    string? Sigla { get; }

    short Cuatrimestre { get; }

    short Orden { get; }

    bool EsAnual { get; }

    bool AdmitePromocion { get; }

    /// <summary>APRSFINAL: aprueba sin rendir final. Excluyente con <see cref="AdmitePromocion"/>.</summary>
    bool ApruebaSinFinal { get; }

    /// <summary>EQUIVALE: código de la materia equivalente (opcional).</summary>
    string? CodigoEquivalencia { get; }

    /// <summary>CORRELATIV: códigos de materias correlativas para cursar.</summary>
    IReadOnlyList<string> CorrelativasCursada { get; }

    /// <summary>CORREFINAL: códigos de materias correlativas para rendir el final.</summary>
    IReadOnlyList<string> CorrelativasFinal { get; }

    /// <summary>ESTADO='B': la materia queda dada de baja (no se borra físicamente).</summary>
    bool DadaDeBaja { get; }
}
