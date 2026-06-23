using Esba.Application.Abstractions;
using Esba.Application.DTOs.Administracion;
using Esba.Application.Features.Administracion;
using Esba.Application.Validators;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using FluentValidation.TestHelper;
using NSubstitute;

namespace Esba.Application.Tests.Administracion;

public class ConfiguracionTests
{
    private readonly IConfiguracionRepository _repo = Substitute.For<IConfiguracionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private ActualizarConfiguracionHandler Handler() =>
        new(_repo, new ActualizarConfiguracionValidator(), _unitOfWork);

    private static ParametroConfiguracion Parametro(string parame, string? valor) =>
        new() { Parame = parame, Descripcion = "desc", Valor = valor };

    private static ActualizarConfiguracionCommand Comando(params (string Parame, string? Valor)[] valores) =>
        new() { Valores = valores.Select(v => new ValorParametro { Parame = v.Parame, Valor = v.Valor }).ToList() };

    [Fact]
    public async Task Actualizar_ValorCambiado_ActualizaYCommiteaUnaVez()
    {
        var entidad = Parametro("Mail_EnvCopia", "viejo@esba");
        _repo.ObtenerPorParamesAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(new[] { entidad });

        var resultado = await Handler().HandleAsync(Comando(("Mail_EnvCopia", "nuevo@esba")), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal(1, resultado.Value);
        Assert.Equal("nuevo@esba", entidad.Valor);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Actualizar_SinCambios_NoCommiteaYDevuelveCero()
    {
        var entidad = Parametro("Mail_EnvCopia", "igual@esba");
        _repo.ObtenerPorParamesAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(new[] { entidad });

        var resultado = await Handler().HandleAsync(Comando(("Mail_EnvCopia", "igual@esba")), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal(0, resultado.Value);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Actualizar_ParametroInexistente_DevuelveWarningSinCommit()
    {
        _repo.ObtenerPorParamesAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ParametroConfiguracion>());

        var resultado = await Handler().HandleAsync(Comando(("No_Existe", "x")), CancellationToken.None);

        Assert.Equal(OperationStatus.Warning, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Actualizar_ListaVacia_DevuelveCeroSinTocarRepositorio()
    {
        var resultado = await Handler().HandleAsync(Comando(), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal(0, resultado.Value);
        await _repo.DidNotReceive().ObtenerPorParamesAsync(
            Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Actualizar_ValorMuyLargo_DevuelveErrorSinTocarRepositorio()
    {
        var resultado = await Handler().HandleAsync(
            Comando(("Mail_EnvCopia", new string('x', 201))), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _repo.DidNotReceive().ObtenerPorParamesAsync(
            Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Validador_ValorDentroDeLimite_EsValido()
    {
        var resultado = new ActualizarConfiguracionValidator()
            .TestValidate(Comando(("Mail_EnvCopia", new string('x', 200))));

        resultado.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validador_ParametroVacio_TieneError()
    {
        var resultado = new ActualizarConfiguracionValidator()
            .TestValidate(Comando(("", "valor")));

        Assert.False(resultado.IsValid);
    }
}
