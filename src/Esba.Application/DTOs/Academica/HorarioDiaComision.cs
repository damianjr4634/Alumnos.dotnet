namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Marcas de bloques (1º/2º/3º) de un día para una comisión, tal como las
/// produce la grilla de horarios del formulario (sucesor del StringGrid de
/// cargacomisiones.pas). Se comprime a COMARM.DIAn/BLOQUEn en el handler.
/// </summary>
public sealed record HorarioDiaComision
{
    public required string Dia { get; init; }

    public bool Primero { get; init; }

    public bool Segundo { get; init; }

    public bool Tercero { get; init; }

    public bool TieneDictado => Primero || Segundo || Tercero;
}
