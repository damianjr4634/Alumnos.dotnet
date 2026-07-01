using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Domain.Academica;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Academica;

/// <summary>
/// Caso de uso de la regularización de CNA: deriva la condición de la nota final (dominio)
/// y vuelca a CURSADA/ANALITIC. Sucesor de GrabaMateriaCNAClick + el commit XXX_REGULARIZACION
/// (rama BAC). La condición la decide el servidor.
/// </summary>
public sealed class ConfirmarRegularizacionCnaHandler
{
    private readonly IValidator<ConfirmarRegularizacionCnaCommand> _validator;
    private readonly IRegularizacionRepository _repositorio;

    public ConfirmarRegularizacionCnaHandler(
        IValidator<ConfirmarRegularizacionCnaCommand> validator,
        IRegularizacionRepository repositorio)
    {
        _validator = validator;
        _repositorio = repositorio;
    }

    public async Task<Result<int>> HandleAsync(ConfirmarRegularizacionCnaCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<int>(validacion.Errors[0].ErrorMessage);
        }

        var resueltas = command.Filas.Select(fila => new FilaRegularizacionCnaResuelta
        {
            CodigoAlumno = fila.CodigoAlumno,
            CodigoMateria = fila.CodigoMateria,
            CuatrimestreAnio = fila.CuatrimestreAnio,
            NuevaCondicion = CalculoCondicionRegularizacionCna.Resolver(fila.NotaFinal),
            NotaFinal = fila.NotaFinal ?? 0m,
            Fecha = fila.Fecha,
        }).ToList();

        try
        {
            var procesadas = await _repositorio.ConfirmarCnaAsync(
                command.CodigoCarrera, command.CodigoUsuario, resueltas, ct).ConfigureAwait(false);
            return Result.Ok(procesadas);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Error<int>(ex.Message);
        }
    }

    /// <summary>Condición resultante de una nota final (reusado por la vista previa de la UI).</summary>
    public static string Resolver(decimal? notaFinal) => CalculoCondicionRegularizacionCna.Resolver(notaFinal);
}
