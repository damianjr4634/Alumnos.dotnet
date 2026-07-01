using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Domain.Academica;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Academica;

/// <summary>
/// Caso de uso de la regularización de secundario (333/650): valida las notas, resuelve la
/// condición de cada materia con el dominio (2° trimestre + exámenes de diciembre/marzo) y
/// vuelca a CURSADA/ANALITIC. Sucesor de la grabación por materia + XXX_REGULARIZACION_MAT_333
/// + el commit XXX_REGULARIZACION (rama 333/650), sin el staging "$$$CURSADA". La condición
/// la decide el servidor.
/// </summary>
public sealed class ConfirmarRegularizacion333Handler
{
    private const string CondicionPreviaManual = "PREVIA";

    private readonly IValidator<ConfirmarRegularizacion333Command> _validator;
    private readonly IRegularizacionRepository _repositorio;

    public ConfirmarRegularizacion333Handler(
        IValidator<ConfirmarRegularizacion333Command> validator,
        IRegularizacionRepository repositorio)
    {
        _validator = validator;
        _repositorio = repositorio;
    }

    public async Task<Result<int>> HandleAsync(ConfirmarRegularizacion333Command command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<int>(validacion.Errors[0].ErrorMessage);
        }

        var resueltas = new List<FilaRegularizacion333Resuelta>(command.Filas.Count);
        foreach (var fila in command.Filas)
        {
            string condicion;
            decimal notaFinal;
            DateTime? notaFinalFecha;
            var notaMar = fila.NotaMar;

            if (fila.ForzarPrevia)
            {
                // Override manual "A previa": marca el examen de marzo pendiente (NOTAMAR=99).
                condicion = CondicionPreviaManual;
                notaFinal = 0m;
                notaFinalFecha = null;
                notaMar = 99m;
            }
            else
            {
                var resultado = Resolver(fila);
                if (resultado.FaltaFecha)
                {
                    // Réplica del FERRCOD=2: si diciembre/marzo aprueban falta cargar su fecha.
                    return Result.Error<int>(
                        $"La materia {fila.CodigoMateria} aprueba por diciembre/marzo: cargá la fecha del examen.");
                }

                condicion = resultado.Condicion;
                notaFinal = resultado.NotaFinal;
                notaFinalFecha = resultado.NotaFinalFecha;
            }

            resueltas.Add(new FilaRegularizacion333Resuelta
            {
                CodigoAlumno = fila.CodigoAlumno,
                CodigoMateria = fila.CodigoMateria,
                CuatrimestreAnio = fila.CuatrimestreAnio,
                TpEva = fila.TpEva,
                TpEva2 = fila.TpEva2,
                TpEva3 = fila.TpEva3,
                FecEva1 = fila.FecEva1,
                FecEva2 = fila.FecEva2,
                FecEva3 = fila.FecEva3,
                NotaDic = fila.NotaDic,
                FechDic = fila.FechDic,
                NotaMar = notaMar,
                FechMar = fila.FechMar,
                TotalHoras = fila.TotalHoras,
                Inasistencias = fila.Inasistencias,
                Justificadas = fila.Justificadas,
                Fecha = fila.Fecha,
                NuevaCondicion = condicion,
                NotaFinal = notaFinal,
                NotaFinalFecha = notaFinalFecha,
            });
        }

        try
        {
            var procesadas = await _repositorio.Confirmar333Async(
                command.CodigoCarrera, command.CodigoUsuario, resueltas, ct).ConfigureAwait(false);
            return Result.Ok(procesadas);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Error<int>(ex.Message);
        }
    }

    /// <summary>Resuelve condición + nota final de una fila (reusado por la vista previa de la UI).</summary>
    public static CalculoCondicionRegularizacion333.Resultado Resolver(NotaCursado333Input fila) =>
        CalculoCondicionRegularizacion333.Resolver(
            new NotasRegularizacion333(
                fila.CondicionActual,
                fila.TpEva,
                fila.TpEva2,
                fila.NotaDic,
                fila.NotaMar,
                fila.FecEva2,
                fila.FechDic,
                fila.FechMar));
}
