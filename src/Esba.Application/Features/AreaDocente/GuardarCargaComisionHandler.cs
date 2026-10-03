using Esba.Application.Abstractions;
using Esba.Application.DTOs.AreaDocente;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using FluentValidation;

namespace Esba.Application.Features.AreaDocente;

/// <summary>
/// Guarda el borrador de una comisión (hito 19). Verifica que la comisión exista en
/// COMARM y que el actor pueda editar (titular en borrador, o secretaría); crea la
/// cabecera en el primer guardado (con el titular de ese momento) y hace upsert de los
/// detalles por alumno: una fila vacía que no existía no se crea; una existente se
/// actualiza aunque quede vacía (el docente puede borrar lo que había cargado).
/// Una sola transacción (SaveChanges). Devuelve el ID de la carga.
/// </summary>
public sealed class GuardarCargaComisionHandler
{
    private readonly ICargaComisionDocenteRepository _cargas;
    private readonly ICargaComisionDocenteQuery _consulta;
    private readonly IValidator<GuardarCargaComisionCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _reloj;

    public GuardarCargaComisionHandler(
        ICargaComisionDocenteRepository cargas,
        ICargaComisionDocenteQuery consulta,
        IValidator<GuardarCargaComisionCommand> validator,
        IUnitOfWork unitOfWork,
        TimeProvider reloj)
    {
        _cargas = cargas;
        _consulta = consulta;
        _validator = validator;
        _unitOfWork = unitOfWork;
        _reloj = reloj;
    }

    public async Task<Result<int>> HandleAsync(GuardarCargaComisionCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<int>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        var comision = await _consulta.ObtenerAsync(command.Comision, ct).ConfigureAwait(false);
        if (comision is null)
        {
            return Result.Error<int>("La comisión no existe.");
        }

        var carga = await _cargas.ObtenerPorComisionAsync(command.Comision, ct).ConfigureAwait(false);

        var motivo = AutorizacionCargaDocente.MotivoNoPuedeEditar(command.Actor, comision.CodigoDocenteTitular, carga?.Estado);
        if (motivo is not null)
        {
            return Result.Error<int>(motivo);
        }

        // Solo alumnos que realmente están en la comisión: lo demás se ignora (la grilla
        // no debería mandarlos, pero el servidor no confía en la UI).
        var alumnosDeLaComision = comision.Alumnos.ToDictionary(a => a.CodigoAlumno, StringComparer.OrdinalIgnoreCase);

        var ahora = _reloj.GetLocalNow().DateTime;
        var usuario = command.Actor.CodigoUsuario;

        if (carga is null)
        {
            if (string.IsNullOrWhiteSpace(comision.CodigoDocenteTitular))
            {
                return Result.Error<int>("La comisión no tiene docente titular asignado: no se puede iniciar la precarga.");
            }

            carga = new CargaComisionDocente
            {
                CodigoCarrera = comision.CodigoCarrera,
                Cutuco = comision.Cutuco,
                CodigoMateria = comision.CodigoMateria,
                CuatrimestreAnio = comision.CuatrimestreAnio,
                CodigoDocente = comision.CodigoDocenteTitular,
                FechaAlta = ahora,
                CodigoUsuarioAlta = usuario,
            };
            _cargas.Agregar(carga);
        }

        carga.Observaciones = command.Observaciones?.Trim() is { Length: > 0 } obs ? obs : null;
        carga.RegistrarModificacion(usuario, ahora);

        var existentes = carga.Detalles.ToDictionary(d => d.CodigoAlumno.Trim(), StringComparer.OrdinalIgnoreCase);
        foreach (var fila in command.Filas)
        {
            var codigoAlumno = fila.CodigoAlumno.Trim();
            if (!alumnosDeLaComision.TryGetValue(codigoAlumno, out var alumno))
            {
                continue;
            }

            var detalle = existentes.GetValueOrDefault(codigoAlumno);
            if (detalle is null)
            {
                detalle = new CargaComisionDocenteDetalle { CodigoAlumno = codigoAlumno };
                Copiar(fila, detalle);
                if (detalle.EstaVacio)
                {
                    continue;
                }

                carga.Detalles.Add(detalle);
            }
            else
            {
                Copiar(fila, detalle);
            }

            detalle.CursadaIndice = alumno.CursadaIndice;
            detalle.FechaModificacion = ahora;
            detalle.CodigoUsuarioModificacion = usuario;
        }

        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(carga.Id);
    }

    private static void Copiar(FilaCargaComisionInput origen, CargaComisionDocenteDetalle destino)
    {
        destino.Evaluacion1 = origen.Evaluacion1;
        destino.Recuperatorio1 = origen.Recuperatorio;
        destino.Evaluacion2 = origen.Evaluacion2;
        destino.Evaluacion3 = origen.Evaluacion3;
        destino.NotaRegular = origen.NotaRegular;
        destino.NotaFinal = origen.NotaFinal;
        destino.TotalHoras = origen.TotalHoras;
        destino.Inasistencias = origen.Inasistencias;
        destino.Justificadas = origen.Justificadas;
        destino.Observaciones = origen.Observaciones?.Trim() is { Length: > 0 } obs ? obs : null;
    }
}
