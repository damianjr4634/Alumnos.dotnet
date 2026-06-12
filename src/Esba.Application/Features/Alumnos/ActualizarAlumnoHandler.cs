using Esba.Application.Abstractions;
using Esba.Application.DTOs.Alumnos;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using FluentValidation;

namespace Esba.Application.Features.Alumnos;

/// <summary>
/// Modificación de alumno (sucesor del UPDATE de FrmAltaAlumno.grabarClick en
/// modo Modificacion). No toca COD_ALU/CARRE/MATRIZ (cambio de DNI y libro
/// matriz tienen su propio caso de uso vía SP XXX_CAMBIA_DNI_LM); a diferencia
/// del alta, permite marcar/levantar la baja. La foto no se borra: si el
/// command trae null se conserva la existente (regla del trigger ALUMNOS_BIU0).
/// </summary>
public sealed class ActualizarAlumnoHandler
{
    private readonly IAlumnoRepository _alumnos;
    private readonly IValidator<ActualizarAlumnoCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarAlumnoHandler(
        IAlumnoRepository alumnos,
        IValidator<ActualizarAlumnoCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _alumnos = alumnos;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> HandleAsync(ActualizarAlumnoCommand command, string usuario, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<string>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        var alumno = await _alumnos.ObtenerAsync(command.CodigoCarrera, command.Codigo, ct).ConfigureAwait(false);
        if (alumno is null)
        {
            return Result.Error<string>("El alumno no existe.");
        }

        AplicarDatos(alumno, command);
        alumno.Usuario = usuario;

        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(alumno.Codigo);
    }

    private static void AplicarDatos(Alumno alumno, ActualizarAlumnoCommand c)
    {
        alumno.Apellido = c.Apellido;
        alumno.Nombre = c.Nombre;
        alumno.DocumentoExpedidoPor = c.DocumentoExpedidoPor;
        alumno.Sexo = c.Sexo;
        alumno.Nacionalidad = c.Nacionalidad;
        alumno.EstadoCivil = c.EstadoCivil;
        alumno.FechaNacimiento = c.FechaNacimiento;
        alumno.LugarNacimiento = c.LugarNacimiento;
        alumno.ProvinciaNacimiento = c.ProvinciaNacimiento;
        alumno.Domicilio = c.Domicilio;
        alumno.Localidad = c.Localidad;
        alumno.CodigoPostal = c.CodigoPostal;
        alumno.CaracteristicaTelefono = c.CaracteristicaTelefono;
        alumno.Telefono = c.Telefono;
        alumno.Celular = c.Celular;
        alumno.Mail = c.Mail;
        alumno.FechaIngreso = c.FechaIngreso;
        alumno.ColegioPrimario = c.ColegioPrimario;
        alumno.AnioPrimario = c.AnioPrimario;
        alumno.TituloPrimario = c.TituloPrimario;
        alumno.ColegioSecundario = c.ColegioSecundario;
        alumno.AnioSecundario = c.AnioSecundario;
        alumno.TituloSecundario = c.TituloSecundario;
        alumno.InstitucionTerciaria = c.InstitucionTerciaria;
        alumno.AnioTerciario = c.AnioTerciario;
        alumno.TituloTerciario = c.TituloTerciario;
        alumno.Empresa = c.Empresa;
        alumno.Rubro = c.Rubro;
        alumno.Cargo = c.Cargo;
        alumno.Antiguedad = c.Antiguedad;
        alumno.DomicilioLaboral = c.DomicilioLaboral;
        alumno.TelefonoLaboral = c.TelefonoLaboral;
        alumno.InternoLaboral = c.InternoLaboral;
        alumno.TieneCertificadoEnTramite = c.TieneCertificadoEnTramite;
        alumno.FechaCertificadoEnTramite = c.FechaCertificadoEnTramite;
        alumno.SituacionDni = c.SituacionDni;
        alumno.PresentoCa = c.PresentoCa;
        alumno.Baja = c.Baja;
        alumno.Observaciones = c.Observaciones;
        alumno.PresentoFoto = c.PresentoFoto;
        alumno.PresentoAptoFisico = c.PresentoAptoFisico;
        alumno.FechaAptoFisico = c.FechaAptoFisico;
        alumno.UsuarioWeb = c.UsuarioWeb;
        alumno.PresentoPartidaNacimiento = c.PresentoPartidaNacimiento;
        alumno.PresentoLibreta = c.PresentoLibreta;
        alumno.FechaLibreta = c.FechaLibreta;
        alumno.Estado = c.Estado;
        alumno.Genero = c.Genero;
        alumno.NominaPase = c.NominaPase;

        // La foto solo se actualiza si viene una nueva: el trigger legacy
        // ALUMNOS_BIU0 prohíbe borrarla.
        if (c.Foto is not null)
        {
            alumno.Foto = c.Foto;
        }
    }
}
