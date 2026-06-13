using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using FluentValidation;

namespace Esba.Application.Features.Academica;

/// <summary>
/// Inscribe a un alumno en una materia de su carrera (sucesor del INSERT de
/// InscripcionDeMaterias.GrabaMateriaClick). Replica el armado legacy:
/// CUTUCO = 900 + turno×10 + comisión (el trigger CURSADA_BI0 resuelve el 9),
/// NREG = 0, y APELLIDO/MATRIZ del alumno + INSTITUT/CARAC de la carrera.
/// El duplicado se valida explícitamente (el legacy dependía de la PK).
/// </summary>
public sealed class InscribirEnMateriaHandler
{
    private readonly ICursadaRepository _cursadas;
    private readonly IAlumnoRepository _alumnos;
    private readonly IMateriaRepository _materias;
    private readonly IValidator<InscribirEnMateriaCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public InscribirEnMateriaHandler(
        ICursadaRepository cursadas,
        IAlumnoRepository alumnos,
        IMateriaRepository materias,
        IValidator<InscribirEnMateriaCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _cursadas = cursadas;
        _alumnos = alumnos;
        _materias = materias;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> HandleAsync(InscribirEnMateriaCommand command, string usuario, CancellationToken ct)
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

        var materia = await _materias.ObtenerAsync(command.CodigoMateria, command.CodigoCarrera, ct).ConfigureAwait(false);
        if (materia is null)
        {
            return Result.Error<string>("La materia no existe en la carrera.");
        }

        var existente = await _cursadas.ObtenerAsync(command.CodigoCarrera, command.CodigoAlumno, command.CodigoMateria, ct).ConfigureAwait(false);
        if (existente is not null)
        {
            return Result.Error<string>($"El alumno ya está inscripto en {materia.Nombre?.Trim() ?? command.CodigoMateria} (condición {existente.Condicion.Trim()}).");
        }

        var cursada = new Cursada
        {
            CodigoCarrera = command.CodigoCarrera,
            CodigoAlumno = command.CodigoAlumno,
            CodigoMateria = command.CodigoMateria,
            Apellido = alumno.Apellido,
            Matriz = alumno.Matriz,
            Instituto = alumno.Carrera?.Instituto,
            Caracteristica = alumno.Carrera?.Caracteristica,
            Cutuco = ArmarCutuco(command.Turno, command.NumeroComision),
            CuatrimestreAnio = command.CuatrimestreAnio,
            Condicion = command.Condicion.Trim().ToUpperInvariant(),
            NumeroRegistro = 0,
            Usuario = usuario,
        };

        _cursadas.Agregar(cursada);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(command.CodigoMateria);
    }

    /// <summary>900 + turno×10 + comisión: el 9 lo resuelve el trigger con MATERIAS.CUATRIM.</summary>
    internal static short ArmarCutuco(int turno, int comision) => (short)(900 + (turno * 10) + comision);
}
