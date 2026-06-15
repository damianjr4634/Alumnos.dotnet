using Esba.Application.DTOs.Academica;
using Esba.Domain.Entities;

namespace Esba.Application.Features.Academica;

/// <summary>
/// Mapeo de los campos editables de materia hacia la entidad, común al alta y a
/// la modificación. Encapsula las conversiones del legacy: 'S'/'N', ESTADO 'B'/'Y'
/// y las correlativas unidas por '-'.
/// </summary>
internal static class MateriaMapping
{
    /// <summary>Normaliza el código a 2 dígitos con ceros a la izquierda (Lpad legacy).</summary>
    public static string NormalizarCodigo(string codigo) => codigo.Trim().PadLeft(2, '0');

    public static void Aplicar(Materia materia, IMateriaCampos campos, string usuario)
    {
        materia.Nombre = campos.Nombre;
        materia.Sigla = campos.Sigla;
        materia.Cuatrimestre = campos.Cuatrimestre;
        materia.Orden = campos.Orden;
        materia.EsAnual = campos.EsAnual;
        materia.AdmitePromocion = campos.AdmitePromocion;
        materia.ApruebaSinFinal = campos.ApruebaSinFinal ? "S" : "N";
        materia.CodigoEquivalencia = string.IsNullOrWhiteSpace(campos.CodigoEquivalencia)
            ? null
            : NormalizarCodigo(campos.CodigoEquivalencia);
        materia.CorrelativasCursada = Unir(campos.CorrelativasCursada);
        materia.CorrelativasFinal = Unir(campos.CorrelativasFinal);
        materia.Estado = campos.DadaDeBaja ? "B" : "Y";
        materia.Usuario = usuario;
    }

    /// <summary>Une los códigos de correlativas con '-' (formato legacy CORRELATIV/CORREFINAL).</summary>
    private static string? Unir(IReadOnlyList<string> codigos) =>
        codigos.Count == 0 ? null : string.Join("-", codigos.Select(c => c.Trim()));
}
