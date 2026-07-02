using System.Globalization;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Domain.Academica;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Academica;

/// <summary>
/// Caso de uso de la regularización terciaria: valida las notas del cursado, resuelve
/// la condición de cada materia con el dominio y vuelca a CURSADA/ANALITIC. Sucesor de
/// ValidoGrabaciondeMateria + el commit XXX_REGULARIZACION, sin el staging "$$$CURSADA".
/// La condición la decide el servidor (autoridad), no el cliente.
/// </summary>
public sealed class ConfirmarRegularizacionHandler
{
    private const string ParametroNotaPromocion = "Regula_NotPromocion";

    private readonly IValidator<ConfirmarRegularizacionCommand> _validator;
    private readonly IConfiguracionQuery _configuracion;
    private readonly IRegularizacionRepository _repositorio;

    public ConfirmarRegularizacionHandler(
        IValidator<ConfirmarRegularizacionCommand> validator,
        IConfiguracionQuery configuracion,
        IRegularizacionRepository repositorio)
    {
        _validator = validator;
        _configuracion = configuracion;
        _repositorio = repositorio;
    }

    /// <summary>Umbral de promoción (XXX_CONF), para la vista previa de condición en la UI.</summary>
    public async Task<decimal> ObtenerNotaPromocionAsync(CancellationToken ct)
    {
        var valor = await _configuracion.ObtenerValorAsync(ParametroNotaPromocion, ct).ConfigureAwait(false);
        return decimal.TryParse(valor, NumberStyles.Number, CultureInfo.InvariantCulture, out var n) ? n : 0m;
    }

    public async Task<Result<int>> HandleAsync(ConfirmarRegularizacionCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<int>(validacion.Errors[0].ErrorMessage);
        }

        var notaPromocion = await ObtenerNotaPromocionAsync(ct).ConfigureAwait(false);

        var resueltas = command.Filas.Select(fila =>
        {
            if (fila.ForzarLibre)
            {
                return ForzarLibre(fila);
            }

            var resultado = Resolver(fila, notaPromocion);
            return new FilaRegularizacionResuelta
            {
                CodigoAlumno = fila.CodigoAlumno,
                CodigoMateria = fila.CodigoMateria,
                CuatrimestreAnio = fila.CuatrimestreAnio,
                TpEva = fila.TpEva,
                TpEva2 = fila.TpEva2,
                Recuperatorio = fila.Recuperatorio,
                TotalHoras = fila.TotalHoras,
                Inasistencias = fila.Inasistencias,
                Justificadas = fila.Justificadas,
                NuevaCondicion = resultado.Condicion,
                NotaAnalitico = resultado.NotaAnalitico,
            };
        }).ToList();

        try
        {
            var procesadas = await _repositorio.ConfirmarTerciariaAsync(
                command.CodigoCarrera, command.CodigoUsuario, resueltas, ct).ConfigureAwait(false);
            return Result.Ok(procesadas);
        }
        catch (InvalidOperationException ex)
        {
            // Ej.: falta la fecha de promoción del cuatrimestre en TBL_CUAT.
            return Result.Error<int>(ex.Message);
        }
    }

    // Override manual "pasar a Libre" (BtnLibre del formulario legacy por-alumno): fuerza
    // CONDICION=LIBRE y las notas/faltas a 99, sin cálculo. No va al analítico (no es aprobación).
    private static FilaRegularizacionResuelta ForzarLibre(NotaCursadoInput fila) => new()
    {
        CodigoAlumno = fila.CodigoAlumno,
        CodigoMateria = fila.CodigoMateria,
        CuatrimestreAnio = fila.CuatrimestreAnio,
        TpEva = 99m,
        TpEva2 = 99m,
        Recuperatorio = 99m,
        TotalHoras = 99,
        Inasistencias = 99,
        Justificadas = fila.Justificadas,
        NuevaCondicion = "LIBRE",
        NotaAnalitico = null,
    };

    /// <summary>Resuelve condición + nota de analítico de una fila (reusado por la vista previa).</summary>
    public static CalculoCondicionRegularizacionTerciaria.Resultado Resolver(NotaCursadoInput fila, decimal notaPromocion) =>
        CalculoCondicionRegularizacionTerciaria.Resolver(
            new NotasRegularizacionTerciaria(
                fila.CondicionActual,
                fila.TpEva,
                fila.TpEva2,
                fila.Recuperatorio,
                fila.TotalHoras,
                fila.Inasistencias,
                fila.Justificadas,
                fila.MateriaPromociona,
                fila.MateriaApruebaSinFinal),
            notaPromocion);
}
