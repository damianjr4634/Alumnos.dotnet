using Esba.Application.Abstractions;
using Esba.Application.DTOs.Administracion;
using Esba.Application.Features.Administracion;
using Esba.Application.Validators;
using Esba.Domain.Common;
using FluentValidation.TestHelper;
using NSubstitute;

namespace Esba.Application.Tests.Administracion;

public class EnviarCorreoPruebaTests
{
    private readonly IEmailService _email = Substitute.For<IEmailService>();

    private EnviarCorreoPruebaHandler Handler() =>
        new(_email, new EnviarCorreoPruebaValidator());

    [Fact]
    public async Task Enviar_DestinatarioValido_DelegaEnEmailServiceUnaVez()
    {
        _email.EnviarAsync(Arg.Any<MensajeCorreo>(), Arg.Any<CancellationToken>()).Returns(Result.Ok(true));

        var resultado = await Handler().HandleAsync(
            new EnviarCorreoPruebaCommand { Destinatario = "destino@esba.edu.ar" }, CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        await _email.Received(1).EnviarAsync(
            Arg.Is<MensajeCorreo>(m => m.Para.Single() == "destino@esba.edu.ar" && !m.EsHtml),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Enviar_DireccionInvalida_DevuelveErrorSinLlamarAlServicio()
    {
        var resultado = await Handler().HandleAsync(
            new EnviarCorreoPruebaCommand { Destinatario = "no-es-mail" }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _email.DidNotReceive().EnviarAsync(Arg.Any<MensajeCorreo>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Enviar_FalloDelServicio_PropagaElError()
    {
        _email.EnviarAsync(Arg.Any<MensajeCorreo>(), Arg.Any<CancellationToken>())
            .Returns(Result.Error<bool>("No se pudo enviar el correo: host desconocido."));

        var resultado = await Handler().HandleAsync(
            new EnviarCorreoPruebaCommand { Destinatario = "destino@esba.edu.ar" }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Contains("host desconocido", resultado.Message);
    }

    [Fact]
    public void Validador_CorreoValido_EsValido()
    {
        new EnviarCorreoPruebaValidator()
            .TestValidate(new EnviarCorreoPruebaCommand { Destinatario = "ok@esba.edu.ar" })
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validador_Vacio_TieneError()
    {
        new EnviarCorreoPruebaValidator()
            .TestValidate(new EnviarCorreoPruebaCommand { Destinatario = "" })
            .ShouldHaveValidationErrorFor(c => c.Destinatario);
    }
}
