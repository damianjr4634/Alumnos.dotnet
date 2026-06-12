using Esba.Application.Abstractions;
using Esba.Application.DTOs.Alumnos;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using Esba.Domain.ValueObjects;
using FluentValidation;

namespace Esba.Application.Features.Alumnos;

/// <summary>
/// Alta de alumno (sucesor del INSERT de FrmAltaAlumno.grabarClick). Replica
/// el control de duplicados del legacy (documentoKeyPress): si el (carrera,
/// código) ya existe activo → "ya existe en altas"; si existe de baja →
/// "ya existe en bajas" con el nombre. El alta siempre nace con BAJA='N'.
/// Devuelve el COD_ALU generado (tipo doc + número con ceros).
/// </summary>
public sealed class CrearAlumnoHandler
{
    private readonly IAlumnoRepository _alumnos;
    private readonly IValidator<CrearAlumnoCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public CrearAlumnoHandler(
        IAlumnoRepository alumnos,
        IValidator<CrearAlumnoCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _alumnos = alumnos;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> HandleAsync(CrearAlumnoCommand command, string usuario, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<string>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        var codigo = CodigoAlumno.Crear(command.TipoDocumento, command.NumeroDocumento);

        var existente = await _alumnos.ObtenerAsync(command.CodigoCarrera, codigo.Valor, ct).ConfigureAwait(false);
        if (existente is not null)
        {
            return existente.Baja
                ? Result.Error<string>($"El alumno {existente.Apellido?.Trim()}, {existente.Nombre?.Trim()} ya existe en bajas.")
                : Result.Error<string>("El alumno ya existe en altas.");
        }

        var alumno = new Alumno
        {
            Codigo = codigo.Valor,
            CodigoCarrera = command.CodigoCarrera,
            Baja = false,
            Usuario = usuario,
        };
        AplicarDatos(alumno, command);

        _alumnos.Agregar(alumno);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(codigo.Valor);
    }

    private static void AplicarDatos(Alumno alumno, CrearAlumnoCommand c)
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
        alumno.Observaciones = c.Observaciones;
        alumno.PresentoFoto = c.PresentoFoto;
        alumno.PresentoAptoFisico = c.PresentoAptoFisico;
        alumno.FechaAptoFisico = c.FechaAptoFisico;
        alumno.UsuarioWeb = c.UsuarioWeb;
        alumno.Foto = c.Foto;
        alumno.PresentoPartidaNacimiento = c.PresentoPartidaNacimiento;
        alumno.PresentoLibreta = c.PresentoLibreta;
        alumno.FechaLibreta = c.FechaLibreta;
        alumno.Estado = c.Estado;
        alumno.Genero = c.Genero;
        alumno.NominaPase = c.NominaPase;
    }
}
