using Esba.Application.Abstractions;
using Esba.Application.DTOs.Administracion;
using Esba.Application.DTOs.Certificados;
using Esba.Domain.Certificados;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Certificados;

/// <summary>
/// Emite la Constancia de Alumno Regular, sucesor de constanciaalumnoregular.pas. Es
/// de solo lectura (no abre transacción): resuelve el cuatrimestre vigente, verifica
/// que el alumno esté CURSANDO/RECURSANDO (si no, no se puede emitir), arma el texto
/// con el formatter de dominio y delega el PDF al servicio de reporte.
/// </summary>
public sealed class GenerarConstanciaRegularHandler
{
    private readonly IValidator<GenerarConstanciaRegularCommand> _validator;
    private readonly ICuatrimestreVigenteProcedure _cuatrimestre;
    private readonly IConstanciasQuery _constancias;
    private readonly IConstanciaRegularReportService _reporte;
    private readonly IEmailService _email;
    private readonly TimeProvider _tiempo;

    public GenerarConstanciaRegularHandler(
        IValidator<GenerarConstanciaRegularCommand> validator,
        ICuatrimestreVigenteProcedure cuatrimestre,
        IConstanciasQuery constancias,
        IConstanciaRegularReportService reporte,
        IEmailService email,
        TimeProvider tiempo)
    {
        _validator = validator;
        _cuatrimestre = cuatrimestre;
        _constancias = constancias;
        _reporte = reporte;
        _email = email;
        _tiempo = tiempo;
    }

    /// <summary>
    /// Verifica que la constancia se pueda emitir (alumno cursando) sin generar el PDF.
    /// La página lo usa para avisar antes de abrir el reporte.
    /// </summary>
    public async Task<Result<bool>> ValidarAsync(GenerarConstanciaRegularCommand command, CancellationToken ct)
    {
        var datos = await ResolverAsync(command, ct).ConfigureAwait(false);
        return datos.IsSuccess ? Result.Ok(true) : new Result<bool> { Status = datos.Status, Message = datos.Message };
    }

    /// <summary>Re-valida (autoridad del servidor, §2.7) y genera el PDF.</summary>
    public async Task<Result<byte[]>> GenerarPdfAsync(GenerarConstanciaRegularCommand command, CancellationToken ct)
    {
        var datos = await ResolverAsync(command, ct).ConfigureAwait(false);
        if (!datos.IsSuccess || datos.Value is null)
        {
            return new Result<byte[]> { Status = datos.Status, Message = datos.Message };
        }

        return Result.Ok(ConstruirPdf(datos.Value, command));
    }

    /// <summary>
    /// Genera el PDF y lo envía por mail al alumno (sucesor de la opción "Mail" de
    /// BitBtn1Click de constanciaalumnoregular.pas). El alumno debe tener mail cargado.
    /// </summary>
    public async Task<Result<bool>> EnviarPorMailAsync(GenerarConstanciaRegularCommand command, CancellationToken ct)
    {
        var datos = await ResolverAsync(command, ct).ConfigureAwait(false);
        if (!datos.IsSuccess || datos.Value is null)
        {
            return new Result<bool> { Status = datos.Status, Message = datos.Message };
        }

        var alumno = datos.Value.Alumno;
        if (string.IsNullOrWhiteSpace(alumno.Mail))
        {
            return Result.Error<bool>("El alumno no tiene un mail cargado.");
        }

        var pdf = ConstruirPdf(datos.Value, command);

        var mensaje = new MensajeCorreo
        {
            Para = [alumno.Mail.Trim()],
            Asunto = "Constancia de Alumno regular",
            Cuerpo = "Adjuntamos la constancia de alumno regular solicitada.",
            Adjuntos =
            [
                new AdjuntoCorreo
                {
                    NombreArchivo = "Constancia de Alumno Regular.pdf",
                    Contenido = pdf,
                    TipoContenido = "application/pdf",
                },
            ],
        };

        return await _email.EnviarAsync(mensaje, ct).ConfigureAwait(false);
    }

    private byte[] ConstruirPdf(ConstanciaRegularDatos datos, GenerarConstanciaRegularCommand command)
    {
        var (alumno, carrera) = (datos.Alumno, datos.Carrera);
        var cutuco = alumno.Cutuco.ToString(System.Globalization.CultureInfo.InvariantCulture);

        var contexto = new ConstanciaRegularContexto
        {
            NombreCompleto = alumno.NombreCompleto,
            CodigoConPuntos = TextoCastellano.CodigoConPuntos(command.CodigoAlumno),
            NombreCarrera = carrera.Nombre ?? string.Empty,
            CodigoCarrera = command.CodigoCarrera,
            EsADistancia = alumno.EsADistancia,
            Cuatrimestre = PrimerDigito(cutuco),
            Turno = SegundoDigito(cutuco),
            EsCarreraPorAnio = carrera.EsCarreraPorAnio,
            Dictamen = alumno.Dictamen,
            AnteQuien = command.AnteQuien ?? string.Empty,
            Fecha = DateOnly.FromDateTime(_tiempo.GetLocalNow().DateTime),
        };

        var model = new ConstanciaRegularModel
        {
            Titulo = ConstanciaRegularFormatter.Titulo,
            Cuerpo = ConstanciaRegularFormatter.Cuerpo(contexto),
            NotaLegal = ConstanciaRegularFormatter.NotaLegal,
            LineaSubvencion = ConstanciaRegularFormatter.LineaSubvencion(carrera.Tipo),
            Secretaria = carrera.Secretaria,
            Rector = carrera.Rector,
        };

        return _reporte.GenerarConstanciaRegular(model);
    }

    private async Task<Result<ConstanciaRegularDatos>> ResolverAsync(
        GenerarConstanciaRegularCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<ConstanciaRegularDatos>(
                string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        var carrera = await _constancias.ObtenerDatosCarreraAsync(command.CodigoCarrera, ct).ConfigureAwait(false);
        if (carrera is null)
        {
            return Result.Error<ConstanciaRegularDatos>("La carrera no existe.");
        }

        var cuatrimestreVigente = await _cuatrimestre.ObtenerAsync(command.CodigoCarrera, ct).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(cuatrimestreVigente))
        {
            return Result.Error<ConstanciaRegularDatos>(
                "La carrera no tiene un cuatrimestre vigente configurado.");
        }

        var alumno = await _constancias
            .ObtenerAlumnoRegularAsync(command.CodigoAlumno, command.CodigoCarrera, cuatrimestreVigente, ct)
            .ConfigureAwait(false);
        if (alumno is null)
        {
            return Result.Error<ConstanciaRegularDatos>(
                "El alumno no se encuentra cursando actualmente.");
        }

        return Result.Ok(new ConstanciaRegularDatos(alumno, carrera));
    }

    private sealed record ConstanciaRegularDatos(AlumnoRegularDto Alumno, CarreraConstanciaDto Carrera);

    private static int PrimerDigito(string cutuco) =>
        cutuco.Length >= 1 && int.TryParse(cutuco.AsSpan(0, 1), out var d) ? d : 0;

    private static int SegundoDigito(string cutuco) =>
        cutuco.Length >= 2 && int.TryParse(cutuco.AsSpan(1, 1), out var d) ? d : 0;
}
