using Esba.Application.Abstractions;
using Esba.Application.DTOs.Administracion;
using Esba.Application.DTOs.Certificados;
using Esba.Application.Features.Certificados;
using Esba.Application.Validators;
using Esba.Domain.Common;
using NSubstitute;

namespace Esba.Application.Tests.Certificados;

public class GenerarConstanciaRegularHandlerTests
{
    private readonly ICuatrimestreVigenteProcedure _cuatrimestre = Substitute.For<ICuatrimestreVigenteProcedure>();
    private readonly IConstanciasQuery _constancias = Substitute.For<IConstanciasQuery>();
    private readonly IConstanciaRegularReportService _reporte = Substitute.For<IConstanciaRegularReportService>();
    private readonly IEmailService _email = Substitute.For<IEmailService>();

    private GenerarConstanciaRegularHandler Handler() => new(
        new GenerarConstanciaRegularCommandValidator(), _cuatrimestre, _constancias, _reporte, _email, TimeProvider.System);

    private static GenerarConstanciaRegularCommand Comando() => new()
    {
        CodigoCarrera = "ABC",
        CodigoAlumno = "12345",
        AnteQuien = "quien corresponda",
    };

    private void ConfigurarCarreraYCuatrimestre()
    {
        _constancias.ObtenerDatosCarreraAsync("ABC", Arg.Any<CancellationToken>())
            .Returns(new CarreraConstanciaDto { Nombre = "Carrera X", Tipo = "TER", EsCarreraPorAnio = false });
        _cuatrimestre.ObtenerAsync("ABC", Arg.Any<CancellationToken>()).Returns("124");
    }

    private static AlumnoRegularDto AlumnoRegular() => new()
    {
        NombreCompleto = "Pérez, Juan",
        Cutuco = 124,
        EsADistancia = false,
        Dictamen = null,
        Mail = "juan@mail.com",
    };

    [Fact]
    public async Task Generar_AlumnoRegular_DevuelvePdf()
    {
        ConfigurarCarreraYCuatrimestre();
        _constancias.ObtenerAlumnoRegularAsync("12345", "ABC", "124", Arg.Any<CancellationToken>())
            .Returns(AlumnoRegular());
        _reporte.GenerarConstanciaRegular(Arg.Any<ConstanciaRegularModel>()).Returns([1, 2, 3]);

        var resultado = await Handler().GenerarPdfAsync(Comando(), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal([1, 2, 3], resultado.Value);
        _reporte.Received(1).GenerarConstanciaRegular(Arg.Any<ConstanciaRegularModel>());
    }

    [Fact]
    public async Task Generar_AlumnoNoCursando_DevuelveErrorSinReporte()
    {
        ConfigurarCarreraYCuatrimestre();
        _constancias.ObtenerAlumnoRegularAsync("12345", "ABC", "124", Arg.Any<CancellationToken>())
            .Returns((AlumnoRegularDto?)null);

        var resultado = await Handler().GenerarPdfAsync(Comando(), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Contains("no se encuentra cursando", resultado.Message);
        _reporte.DidNotReceive().GenerarConstanciaRegular(Arg.Any<ConstanciaRegularModel>());
    }

    [Fact]
    public async Task Generar_CarreraInexistente_DevuelveError()
    {
        _constancias.ObtenerDatosCarreraAsync("ABC", Arg.Any<CancellationToken>()).Returns((CarreraConstanciaDto?)null);

        var resultado = await Handler().GenerarPdfAsync(Comando(), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _constancias.DidNotReceive().ObtenerAlumnoRegularAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Generar_SinCuatrimestreVigente_DevuelveError()
    {
        _constancias.ObtenerDatosCarreraAsync("ABC", Arg.Any<CancellationToken>())
            .Returns(new CarreraConstanciaDto { Nombre = "Carrera X" });
        _cuatrimestre.ObtenerAsync("ABC", Arg.Any<CancellationToken>()).Returns((string?)null);

        var resultado = await Handler().GenerarPdfAsync(Comando(), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        _reporte.DidNotReceive().GenerarConstanciaRegular(Arg.Any<ConstanciaRegularModel>());
    }

    [Fact]
    public async Task Generar_SinAnteQuien_DevuelveErrorSinConsultar()
    {
        var resultado = await Handler().GenerarPdfAsync(Comando() with { AnteQuien = "" }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _constancias.DidNotReceive().ObtenerDatosCarreraAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Validar_AlumnoRegular_DevuelveOkSinGenerarPdf()
    {
        ConfigurarCarreraYCuatrimestre();
        _constancias.ObtenerAlumnoRegularAsync("12345", "ABC", "124", Arg.Any<CancellationToken>())
            .Returns(AlumnoRegular());

        var resultado = await Handler().ValidarAsync(Comando(), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        _reporte.DidNotReceive().GenerarConstanciaRegular(Arg.Any<ConstanciaRegularModel>());
    }

    [Fact]
    public async Task EnviarPorMail_AlumnoConMail_AdjuntaPdfYEnvia()
    {
        ConfigurarCarreraYCuatrimestre();
        _constancias.ObtenerAlumnoRegularAsync("12345", "ABC", "124", Arg.Any<CancellationToken>())
            .Returns(AlumnoRegular());
        _reporte.GenerarConstanciaRegular(Arg.Any<ConstanciaRegularModel>()).Returns([9, 9, 9]);
        _email.EnviarAsync(Arg.Any<MensajeCorreo>(), Arg.Any<CancellationToken>()).Returns(Result.Ok(true));

        var resultado = await Handler().EnviarPorMailAsync(Comando(), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        await _email.Received(1).EnviarAsync(
            Arg.Is<MensajeCorreo>(m =>
                m.Para.Single() == "juan@mail.com" &&
                m.Adjuntos.Count == 1 &&
                m.Adjuntos[0].TipoContenido == "application/pdf"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnviarPorMail_AlumnoSinMail_DevuelveErrorSinEnviar()
    {
        ConfigurarCarreraYCuatrimestre();
        _constancias.ObtenerAlumnoRegularAsync("12345", "ABC", "124", Arg.Any<CancellationToken>())
            .Returns(AlumnoRegular() with { Mail = null });

        var resultado = await Handler().EnviarPorMailAsync(Comando(), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _email.DidNotReceive().EnviarAsync(Arg.Any<MensajeCorreo>(), Arg.Any<CancellationToken>());
    }
}
