using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using Esba.Domain.Enums;
using FluentValidation;

namespace Esba.Application.Features.Academica;

/// <summary>
/// Da de alta una equivalencia de materia (sucesor de GrabaMateriaClick / página 0 de
/// Equivalencia.pas): inserta en ANALITIC con CONDICION='EQUIVALENCIA'. La numeración
/// interna es autoritativa del servidor (XXX_NUMERO_EQUIVALENCIA) y, si es nueva, se
/// confirma en TBLEQUIVA tras grabar (XXX_GRABA_NUMEQUI); la D.G.E.G.P. la provee el
/// operador. El padding de ACTINT/ACTDGE a 15 ceros lo hace el trigger ANALITIC_BIU.
/// </summary>
public sealed class CrearEquivalenciaHandler
{
    private readonly IValidator<CrearEquivalenciaCommand> _validator;
    private readonly IValidacionMateriaProcedure _validacionMateria;
    private readonly IEquivalenciaNumeracionProcedure _numeracion;
    private readonly IAnaliticoRepository _analiticos;
    private readonly IAlumnoRepository _alumnos;
    private readonly IUnitOfWork _unitOfWork;

    public CrearEquivalenciaHandler(
        IValidator<CrearEquivalenciaCommand> validator,
        IValidacionMateriaProcedure validacionMateria,
        IEquivalenciaNumeracionProcedure numeracion,
        IAnaliticoRepository analiticos,
        IAlumnoRepository alumnos,
        IUnitOfWork unitOfWork)
    {
        _validator = validator;
        _validacionMateria = validacionMateria;
        _numeracion = numeracion;
        _analiticos = analiticos;
        _alumnos = alumnos;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> HandleAsync(CrearEquivalenciaCommand command, string usuario, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<string>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        var alumno = await _alumnos.ObtenerAsync(command.CodigoCarrera, command.CodigoAlumno, ct).ConfigureAwait(false);
        if (alumno is null)
        {
            return Result.Error<string>("El alumno no existe.");
        }

        // TIPO 'A' (modificación de analítico): valida que la materia no esté ya en
        // cursada ni en el analítico (XXX_INSC_VALMAT, FERRCOD=2 → Error).
        var valida = await _validacionMateria
            .ValidarAsync(command.CodigoAlumno, command.CodigoCarrera, command.CodigoMateria, 'A', ct)
            .ConfigureAwait(false);
        if (!valida.IsSuccess)
        {
            return new Result<string> { Status = valida.Status, Message = valida.Message };
        }

        var esInterna = command.TipoActuacion == TipoActuacionEquivalencia.Interna;

        NumeroEquivalenciaDto? numero = null;
        string? actaInterna = null;
        string? actaDgegp = null;
        if (esInterna)
        {
            numero = await _numeracion.ObtenerProximoNumeroAsync(command.CodigoAlumno, command.CodigoCarrera, ct).ConfigureAwait(false);
            actaInterna = numero.NumeroEntero;
        }
        else
        {
            actaDgegp = SinSeparador(command.NumeroDgegp);
        }

        var analitico = new Analitico
        {
            CodigoCarrera = command.CodigoCarrera,
            CodigoAlumno = command.CodigoAlumno,
            CodigoMateria = command.CodigoMateria,
            Apellido = alumno.Apellido,
            Matriz = alumno.Matriz,
            Condicion = "EQUIVALENCIA",
            Instituto = command.InstitutoOrigen,
            Caracteristica = command.CaracteristicaOrigen,
            ActaInterna = actaInterna,
            ActaDge = actaDgegp,
            Colegio = command.Colegio,
            Plan = command.Plan,
            Ac = command.Documento switch
            {
                DocumentoEquivalencia.Constancia => "C",
                DocumentoEquivalencia.Analitico => "A",
                _ => null,
            },
            EquivDocente = command.DocenteOrigen,
            EquivMateria = command.MateriaOrigen,
            EquivCarrera = command.CarreraOrigen,
            EquivInstituto = command.InstitutoOrigen,
            Usuario = usuario,
        };

        _analiticos.Agregar(analitico);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        // Numeración interna nueva: recién acá se confirma el consumo en TBLEQUIVA
        // (el legacy solo lo hace para interna + número nuevo).
        if (esInterna && numero!.EsNuevo && numero.NumeroEntero.Length >= 2 &&
            int.TryParse(numero.NumeroEntero[..^2], out var numeroEntero))
        {
            await _numeracion.GrabarNumeroAsync(numeroEntero, command.CodigoCarrera, ct).ConfigureAwait(false);
        }

        return Result.Ok(command.CodigoMateria);
    }

    // Réplica de Copy(s,1,len-3)+Copy(s,len-1,2) del legacy: quita el separador '/'
    // del número con formato "<n>/AA". El trigger luego rellena con LPAD a 15 ceros.
    private static string? SinSeparador(string? numero)
    {
        var texto = (numero ?? string.Empty).Trim();
        return texto.Length == 0 ? null : texto.Replace("/", string.Empty);
    }
}
