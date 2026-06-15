using Esba.Application.DTOs.Examenes;
using Esba.Domain.Entities;

namespace Esba.Application.Features.Examenes;

/// <summary>
/// Mapeo de los campos de mesa a la entidad, común al alta y a la modificación.
/// Normaliza códigos como el legacy (materia/tipo a 2 dígitos, docentes a 3) y
/// trata 0 como "sin valor" para los numéricos opcionales.
/// </summary>
internal static class MesaMapping
{
    public static void Aplicar(Mesa mesa, IMesaCampos campos, string usuario)
    {
        mesa.CodigoMateria = Normalizar(campos.CodigoMateria, 2);
        mesa.Llamado = (short)campos.Llamado;
        mesa.FechaExamen = campos.FechaExamen;
        mesa.Hora = ShortOpcional(campos.Hora);
        mesa.Titular = Normalizar(campos.Titular, 3);
        mesa.Vocal1 = Normalizar(campos.Vocal1, 3);
        mesa.Vocal2 = Normalizar(campos.Vocal2, 3);
        mesa.Aula = ShortOpcional(campos.Aula);
        mesa.Comision1 = ShortOpcional(campos.Comision1);
        mesa.Comision2 = ShortOpcional(campos.Comision2);
        mesa.Comision3 = ShortOpcional(campos.Comision3);
        mesa.CodigoTipo = Normalizar(campos.CodigoTipo, 2);
        mesa.Usuario = usuario;
    }

    private static string? Normalizar(string? codigo, int largo) =>
        string.IsNullOrWhiteSpace(codigo) ? null : codigo.Trim().PadLeft(largo, '0');

    private static short? ShortOpcional(int valor) => valor == 0 ? null : (short)valor;
}
