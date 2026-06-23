using Esba.Application.Abstractions;
using Esba.Application.DTOs.Administracion;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Administracion;

/// <summary>
/// Guarda los valores editados de la configuración del sistema (XXX_CONF),
/// sucesor del UPDATE por fila de TablaConfiguraciones.pas. Valida, carga los
/// parámetros existentes por PARAME y actualiza solo su VALOR dentro de una única
/// transacción (§1.3). Devuelve la cantidad de parámetros efectivamente guardados.
/// </summary>
public sealed class ActualizarConfiguracionHandler
{
    private readonly IConfiguracionRepository _configuraciones;
    private readonly IValidator<ActualizarConfiguracionCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarConfiguracionHandler(
        IConfiguracionRepository configuraciones,
        IValidator<ActualizarConfiguracionCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _configuraciones = configuraciones;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> HandleAsync(ActualizarConfiguracionCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<int>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        // Último valor gana si el mismo PARAME viniera repetido (defensa).
        var valoresPorParame = command.Valores
            .GroupBy(v => v.Parame.Trim(), StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Last().Valor, StringComparer.Ordinal);

        if (valoresPorParame.Count == 0)
        {
            return Result.Ok(0);
        }

        var entidades = await _configuraciones
            .ObtenerPorParamesAsync(valoresPorParame.Keys.ToList(), ct)
            .ConfigureAwait(false);

        var actualizados = 0;
        foreach (var entidad in entidades)
        {
            var nuevoValor = valoresPorParame[entidad.Parame.Trim()];
            if (entidad.Valor != nuevoValor)
            {
                entidad.Valor = nuevoValor;
                actualizados++;
            }
        }

        if (actualizados > 0)
        {
            await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
        }

        // Parámetros enviados que no existen en la tabla: se ignoran (la pantalla
        // no da de alta parámetros). Avisamos si ninguno coincidió.
        if (entidades.Count == 0)
        {
            return Result.Warning(0, "No se encontró ninguno de los parámetros a guardar.");
        }

        return Result.Ok(actualizados);
    }
}
