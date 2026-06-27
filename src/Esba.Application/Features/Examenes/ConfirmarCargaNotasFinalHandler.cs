using Esba.Application.Abstractions;
using Esba.Application.DTOs.Examenes;
using Esba.Domain.Common;
using Esba.Domain.Examenes;
using FluentValidation;

namespace Esba.Application.Features.Examenes;

/// <summary>
/// Confirma la carga de notas de final de una mesa (sucesor del XXX_MESAS que el
/// legacy disparaba al cerrar FinalesxMesayComision). Calcula por fila la condición
/// resultante y la nota del analítico con <see cref="CalculoCondicionFinal"/>
/// (autoritativo: no confía en lo que muestre la UI) y delega el volcado
/// transaccional al repositorio. Sin SQL ni staging (§2.1, §2.3).
/// </summary>
public sealed class ConfirmarCargaNotasFinalHandler
{
    private readonly ICargaFinalRepository _repositorio;
    private readonly IValidator<CargaNotasFinalCommand> _validator;

    public ConfirmarCargaNotasFinalHandler(
        ICargaFinalRepository repositorio,
        IValidator<CargaNotasFinalCommand> validator)
    {
        _repositorio = repositorio;
        _validator = validator;
    }

    public async Task<Result<int>> HandleAsync(CargaNotasFinalCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<int>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        var esTerciaria = string.Equals(command.TipoCarrera.Trim(), "TER", StringComparison.OrdinalIgnoreCase);

        var resueltas = new List<FilaCargaFinalResuelta>(command.Filas.Count);
        foreach (var fila in command.Filas)
        {
            var condicionActual = fila.CondicionActual?.Trim() ?? string.Empty;

            var resultado = esTerciaria
                ? CalculoCondicionFinal.Terciaria(
                    new NotaFinal(fila.Nota1, fila.Fecha1, fila.Acta1),
                    new NotaFinal(fila.Nota2, fila.Fecha2, fila.Acta2),
                    new NotaFinal(fila.Nota3, fila.Fecha3, fila.Acta3),
                    condicionActual)
                : CalculoCondicionFinal.Bachiller(
                    new NotaFinal(fila.Nota1, fila.Fecha1, fila.Acta1),
                    condicionActual);

            resueltas.Add(new FilaCargaFinalResuelta
            {
                CodigoAlumno = fila.CodigoAlumno,
                CodigoMateria = fila.CodigoMateria,
                EsTerciaria = esTerciaria,
                Nota1 = fila.Nota1,
                Fecha1 = fila.Fecha1,
                Acta1 = fila.Acta1,
                Nota2 = fila.Nota2,
                Fecha2 = fila.Fecha2,
                Acta2 = fila.Acta2,
                Nota3 = fila.Nota3,
                Fecha3 = fila.Fecha3,
                Acta3 = fila.Acta3,
                NuevaCondicion = resultado.Condicion,
                NotaAnalitico = resultado.NotaAnalitico,
                FechaAnalitico = resultado.FechaAnalitico,
                ActaAnalitico = resultado.ActaAnalitico,
            });
        }

        var procesadas = await _repositorio.ConfirmarAsync(
            command.CodigoCarrera, command.Mesa, command.CodigoUsuario,
            command.ConsumirPermiso, resueltas, ct).ConfigureAwait(false);

        return Result.Ok(procesadas);
    }
}
