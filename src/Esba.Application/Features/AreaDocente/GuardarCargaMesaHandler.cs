using Esba.Application.Abstractions;
using Esba.Application.DTOs.AreaDocente;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using FluentValidation;

namespace Esba.Application.Features.AreaDocente;

/// <summary>
/// Guarda el borrador de una mesa (hito 19). Misma mecánica que la comisión: verifica que
/// la mesa exista y que el actor pueda editar (titular en borrador, o secretaría); crea la
/// cabecera en el primer guardado (con el titular de ese momento) y hace upsert de los
/// detalles por alumno+materia, ignorando alumnos sin permiso en la mesa. Una fila vacía
/// nueva no se crea; una existente se actualiza aunque quede vacía.
/// </summary>
public sealed class GuardarCargaMesaHandler
{
    private readonly ICargaMesaDocenteRepository _cargas;
    private readonly ICargaMesaDocenteQuery _consulta;
    private readonly IValidator<GuardarCargaMesaCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _reloj;

    public GuardarCargaMesaHandler(
        ICargaMesaDocenteRepository cargas,
        ICargaMesaDocenteQuery consulta,
        IValidator<GuardarCargaMesaCommand> validator,
        IUnitOfWork unitOfWork,
        TimeProvider reloj)
    {
        _cargas = cargas;
        _consulta = consulta;
        _validator = validator;
        _unitOfWork = unitOfWork;
        _reloj = reloj;
    }

    public async Task<Result<int>> HandleAsync(GuardarCargaMesaCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<int>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        var mesa = await _consulta.ObtenerAsync(command.Mesa, ct).ConfigureAwait(false);
        if (mesa is null)
        {
            return Result.Error<int>("La mesa no existe.");
        }

        var carga = await _cargas.ObtenerPorMesaAsync(command.Mesa, ct).ConfigureAwait(false);

        var motivo = AutorizacionCargaDocente.MotivoNoPuedeEditar(command.Actor, mesa.CodigoDocenteTitular, carga?.Estado, "esta mesa");
        if (motivo is not null)
        {
            return Result.Error<int>(motivo);
        }

        var permisos = mesa.Alumnos.ToDictionary(a => ClaveFila(a.CodigoAlumno, a.CodigoMateria), StringComparer.OrdinalIgnoreCase);

        var ahora = _reloj.GetLocalNow().DateTime;
        var usuario = command.Actor.CodigoUsuario;

        if (carga is null)
        {
            if (string.IsNullOrWhiteSpace(mesa.CodigoDocenteTitular))
            {
                return Result.Error<int>("La mesa no tiene docente titular asignado: no se puede iniciar la precarga.");
            }

            carga = new CargaMesaDocente
            {
                CodigoCarrera = mesa.CodigoCarrera,
                NumeroMesa = mesa.NumeroMesa,
                CodigoDocente = mesa.CodigoDocenteTitular,
                FechaAlta = ahora,
                CodigoUsuarioAlta = usuario,
            };
            _cargas.Agregar(carga);
        }

        carga.Observaciones = command.Observaciones?.Trim() is { Length: > 0 } obs ? obs : null;
        carga.RegistrarModificacion(usuario, ahora);

        var existentes = carga.Detalles.ToDictionary(d => ClaveFila(d.CodigoAlumno, d.CodigoMateria), StringComparer.OrdinalIgnoreCase);
        foreach (var fila in command.Filas)
        {
            var clave = ClaveFila(fila.CodigoAlumno, fila.CodigoMateria);
            if (!permisos.TryGetValue(clave, out var permiso))
            {
                continue;
            }

            var detalle = existentes.GetValueOrDefault(clave);
            if (detalle is null)
            {
                detalle = new CargaMesaDocenteDetalle
                {
                    CodigoAlumno = fila.CodigoAlumno.Trim(),
                    CodigoMateria = fila.CodigoMateria.Trim(),
                };
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

            detalle.PermisoIndice = permiso.PermisoIndice;
            detalle.FechaModificacion = ahora;
            detalle.CodigoUsuarioModificacion = usuario;
        }

        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(carga.Id);
    }

    private static string ClaveFila(string codigoAlumno, string codigoMateria) =>
        $"{codigoAlumno.Trim()}|{codigoMateria.Trim()}";

    private static void Copiar(FilaCargaMesaInput origen, CargaMesaDocenteDetalle destino)
    {
        destino.Ausente = origen.Ausente;
        destino.Nota = origen.Ausente ? null : origen.Nota;
        destino.Observaciones = origen.Observaciones?.Trim() is { Length: > 0 } obs ? obs : null;
    }
}
