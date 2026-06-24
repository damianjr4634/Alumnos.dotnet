using Esba.Application.Abstractions;
using Esba.Application.DTOs.Administracion;
using Esba.Application.Features.Administracion;
using Esba.Application.Validators;
using Esba.Domain.Common;
using FluentValidation.TestHelper;
using NSubstitute;

namespace Esba.Application.Tests.Administracion;

public class EnviarCorreoComisionTests
{
    private readonly IEmailService _email = Substitute.For<IEmailService>();
    private readonly IConfiguracionQuery _configuracion = Substitute.For<IConfiguracionQuery>();

    private EnviarCorreoComisionHandler Handler() =>
        new(_email, _configuracion, new EnviarCorreoComisionValidator());

    private static EnviarCorreoComisionCommand Comando() => new()
    {
        Para = ["a@esba.edu.ar", "b@esba.edu.ar"],
        Asunto = "Aviso de comisión",
        Cuerpo = "Recordatorio de clase.",
        UsuarioRemitente = "secretaria",
    };

    [Fact]
    public async Task Enviar_SinCopiaConfigurada_EnviaUnSoloMensaje()
    {
        _email.EnviarAsync(Arg.Any<MensajeCorreo>(), Arg.Any<CancellationToken>()).Returns(Result.Ok(true));
        _configuracion.ObtenerValorAsync("Mail_EnvCopia", Arg.Any<CancellationToken>()).Returns((string?)null);

        var resultado = await Handler().HandleAsync(Comando(), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        await _email.Received(1).EnviarAsync(Arg.Any<MensajeCorreo>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Enviar_ConCopiaConfigurada_EnviaMensajeYCopiaDeAuditoria()
    {
        _email.EnviarAsync(Arg.Any<MensajeCorreo>(), Arg.Any<CancellationToken>()).Returns(Result.Ok(true));
        _configuracion.ObtenerValorAsync("Mail_EnvCopia", Arg.Any<CancellationToken>()).Returns("auditoria@esba.edu.ar");
        _configuracion.ObtenerValorAsync("Mail_EnvCopiaCC", Arg.Any<CancellationToken>()).Returns((string?)null);
        _configuracion.ObtenerValorAsync("Mail_EnvCopiaCCO", Arg.Any<CancellationToken>()).Returns((string?)null);

        var resultado = await Handler().HandleAsync(Comando(), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        await _email.Received(2).EnviarAsync(Arg.Any<MensajeCorreo>(), Arg.Any<CancellationToken>());
        await _email.Received(1).EnviarAsync(
            Arg.Is<MensajeCorreo>(m => m.Para.Single() == "auditoria@esba.edu.ar"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Enviar_FallaElMensajePrincipal_NoEnviaCopiaYDevuelveError()
    {
        _email.EnviarAsync(Arg.Any<MensajeCorreo>(), Arg.Any<CancellationToken>())
            .Returns(Result.Error<bool>("host desconocido"));

        var resultado = await Handler().HandleAsync(Comando(), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _email.Received(1).EnviarAsync(Arg.Any<MensajeCorreo>(), Arg.Any<CancellationToken>());
        await _configuracion.DidNotReceive().ObtenerValorAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Enviar_FallaSoloLaCopia_DevuelveWarning()
    {
        _email.EnviarAsync(Arg.Is<MensajeCorreo>(m => m.Para.Contains("a@esba.edu.ar")), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(true));
        _email.EnviarAsync(Arg.Is<MensajeCorreo>(m => m.Para.Contains("auditoria@esba.edu.ar")), Arg.Any<CancellationToken>())
            .Returns(Result.Error<bool>("falló copia"));
        _configuracion.ObtenerValorAsync("Mail_EnvCopia", Arg.Any<CancellationToken>()).Returns("auditoria@esba.edu.ar");

        var resultado = await Handler().HandleAsync(Comando(), CancellationToken.None);

        Assert.Equal(OperationStatus.Warning, resultado.Status);
    }

    [Fact]
    public async Task Enviar_SinDestinatarios_DevuelveErrorSinEnviar()
    {
        var resultado = await Handler().HandleAsync(Comando() with { Para = [] }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _email.DidNotReceive().EnviarAsync(Arg.Any<MensajeCorreo>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Validador_SinAsunto_TieneError()
    {
        new EnviarCorreoComisionValidator()
            .TestValidate(Comando() with { Asunto = "" })
            .ShouldHaveValidationErrorFor(c => c.Asunto);
    }

    [Fact]
    public void Validador_ComandoCompleto_EsValido()
    {
        new EnviarCorreoComisionValidator().TestValidate(Comando()).ShouldNotHaveAnyValidationErrors();
    }
}
