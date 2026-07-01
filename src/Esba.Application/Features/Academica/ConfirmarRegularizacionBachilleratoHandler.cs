using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Domain.Academica;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Academica;

/// <summary>
/// Caso de uso de la regularización de bachillerato: valida las notas del cursado,
/// resuelve la condición de cada materia con el dominio (faltas + ladder de notas) y
/// vuelca a CURSADA/ANALITIC. Sucesor de GrabaMateriaBac + _BAC/_POSTVAL + el commit
/// XXX_REGULARIZACION (rama BAC), sin el staging "$$$CURSADA". La condición la decide el
/// servidor (autoridad), no el cliente.
/// </summary>
public sealed class ConfirmarRegularizacionBachilleratoHandler
{
    /// <summary>Condición del override manual "pasar a Libre" (botón del formulario legacy).</summary>
    private const string CondicionLibreManual = "LIBRE";

    private readonly IValidator<ConfirmarRegularizacionBachilleratoCommand> _validator;
    private readonly IRegularizacionRepository _repositorio;

    public ConfirmarRegularizacionBachilleratoHandler(
        IValidator<ConfirmarRegularizacionBachilleratoCommand> validator,
        IRegularizacionRepository repositorio)
    {
        _validator = validator;
        _repositorio = repositorio;
    }

    public async Task<Result<int>> HandleAsync(ConfirmarRegularizacionBachilleratoCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<int>(validacion.Errors[0].ErrorMessage);
        }

        var resueltas = new List<FilaRegularizacionBachilleratoResuelta>(command.Filas.Count);
        foreach (var fila in command.Filas)
        {
            if (fila.ForzarLibre)
            {
                resueltas.Add(ForzarLibre(fila));
                continue;
            }

            var resultado = Resolver(fila);
            if (resultado.RequiereDecision)
            {
                // CONSEJO sin decisión del operador: la UI debe resolverlo antes de confirmar.
                return Result.Error<int>(
                    $"La materia {fila.CodigoMateria} quedó en CONSEJO: elegí Consejo, Regular o Libre antes de procesar.");
            }

            resueltas.Add(new FilaRegularizacionBachilleratoResuelta
            {
                CodigoAlumno = fila.CodigoAlumno,
                CodigoMateria = fila.CodigoMateria,
                CuatrimestreAnio = fila.CuatrimestreAnio,
                TpEva = fila.TpEva,
                TpEva2 = fila.TpEva2,
                Recuperatorio = fila.Recuperatorio,
                NotaRegular = fila.NotaRegular,
                TotalHoras = fila.TotalHoras,
                Inasistencias = fila.Inasistencias,
                Justificadas = fila.Justificadas,
                Fecha = fila.Fecha,
                NuevaCondicion = resultado.Condicion!,
                NotaFinal = resultado.NotaFinal,
            });
        }

        try
        {
            var procesadas = await _repositorio.ConfirmarBachilleratoAsync(
                command.CodigoCarrera, command.CodigoUsuario, resueltas, ct).ConfigureAwait(false);
            return Result.Ok(procesadas);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Error<int>(ex.Message);
        }
    }

    /// <summary>Resuelve condición + derivados de una fila (reusado por la vista previa de la UI).</summary>
    public static CalculoCondicionRegularizacionBachiller.Resultado Resolver(NotaCursadoBachilleratoInput fila) =>
        CalculoCondicionRegularizacionBachiller.Resolver(
            new NotasRegularizacionBachiller(
                fila.CondicionActual,
                fila.TpEva,
                fila.TpEva2,
                fila.Recuperatorio,
                fila.NotaRegular,
                fila.TotalHoras,
                fila.Inasistencias,
                fila.EnRecursa,
                fila.Paso));

    // Override manual "pasar a Libre" (BtnLibre del formulario legacy): fuerza CONDICION=LIBRE
    // y las notas/faltas a 99, sin ladder. No va al analítico (no es REGULAR).
    private static FilaRegularizacionBachilleratoResuelta ForzarLibre(NotaCursadoBachilleratoInput fila) => new()
    {
        CodigoAlumno = fila.CodigoAlumno,
        CodigoMateria = fila.CodigoMateria,
        CuatrimestreAnio = fila.CuatrimestreAnio,
        TpEva = 99m,
        TpEva2 = 99m,
        Recuperatorio = 99m,
        NotaRegular = fila.NotaRegular,
        TotalHoras = 99,
        Inasistencias = 99,
        Justificadas = fila.Justificadas,
        Fecha = fila.Fecha,
        NuevaCondicion = CondicionLibreManual,
        NotaFinal = null,
    };
}
