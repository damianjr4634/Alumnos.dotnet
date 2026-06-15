using Esba.Application.Abstractions;
using Esba.Application.DTOs.Examenes;
using Esba.Application.Features.Examenes;
using Esba.Application.Validators;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using FluentValidation.TestHelper;
using NSubstitute;

namespace Esba.Application.Tests.Examenes;

public class MesaValidatorTests
{
    private readonly CrearMesaValidator _validator = new();

    private static CrearMesaCommand Valido() => new()
    {
        CodigoCarrera = "ADM",
        NumeroMesa = 10,
        CodigoMateria = "01",
        FechaExamen = new DateOnly(2026, 7, 1),
        CodigoTipo = "01",
        Llamado = 1,
    };

    [Fact]
    public void Validar_Valido_Pasa() => _validator.TestValidate(Valido()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validar_SinMateria_Falla() =>
        _validator.TestValidate(Valido() with { CodigoMateria = "" }).ShouldHaveValidationErrorFor(m => m.CodigoMateria);

    [Fact]
    public void Validar_SinTipo_Falla() =>
        _validator.TestValidate(Valido() with { CodigoTipo = "" }).ShouldHaveValidationErrorFor(m => m.CodigoTipo);

    [Fact]
    public void Validar_MesaCero_Falla() =>
        _validator.TestValidate(Valido() with { NumeroMesa = 0 }).ShouldHaveValidationErrorFor(m => m.NumeroMesa);
}

public class MesaHandlersTests
{
    private readonly IMesaRepository _mesas = Substitute.For<IMesaRepository>();
    private readonly IValidoMesaProcedure _validoMesa = Substitute.For<IValidoMesaProcedure>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private CrearMesaHandler CrearHandler() => new(_mesas, _validoMesa, new CrearMesaValidator(), _unitOfWork);

    private ActualizarMesaHandler ActualizarHandler() => new(_mesas, new ActualizarMesaValidator(), _unitOfWork);

    private EliminarMesaHandler EliminarHandler() => new(_mesas, _unitOfWork);

    private static CrearMesaCommand Alta() => new()
    {
        CodigoCarrera = "ADM",
        NumeroMesa = 10,
        CodigoMateria = "1",
        FechaExamen = new DateOnly(2026, 7, 1),
        CodigoTipo = "1",
        Llamado = 1,
        Titular = "7",
        Hora = 0,
    };

    private void SinDuplicado() =>
        _validoMesa.VerificarAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(true));

    [Fact]
    public async Task Crear_NuevaMesa_AgregaYNormaliza()
    {
        SinDuplicado();
        Mesa? capturada = null;
        _mesas.When(m => m.Agregar(Arg.Any<Mesa>())).Do(ci => capturada = ci.Arg<Mesa>());

        var resultado = await CrearHandler().HandleAsync(Alta(), "tester", CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal("01", capturada!.CodigoMateria);   // LPad 2
        Assert.Equal("007", capturada.Titular);          // LPad 3
        Assert.Equal("01", capturada.CodigoTipo);
        Assert.Null(capturada.Hora);                     // 0 → null
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_MesaDuplicada_NoAgrega()
    {
        _validoMesa.VerificarAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Error<bool>("La mesa ya existe"));

        var resultado = await CrearHandler().HandleAsync(Alta(), "tester", CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        _mesas.DidNotReceive().Agregar(Arg.Any<Mesa>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_Invalido_NoChequeaDuplicado()
    {
        var resultado = await CrearHandler().HandleAsync(Alta() with { CodigoTipo = "" }, "tester", CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _validoMesa.DidNotReceive().VerificarAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Actualizar_Inexistente_DevuelveError()
    {
        _mesas.ObtenerAsync("ADM", 10, Arg.Any<CancellationToken>()).Returns((Mesa?)null);

        var resultado = await ActualizarHandler().HandleAsync(new ActualizarMesaCommand
        {
            CodigoCarrera = "ADM", NumeroMesa = 10, CodigoMateria = "01",
            FechaExamen = new DateOnly(2026, 7, 1), CodigoTipo = "01", Llamado = 1,
        }, "tester", CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Eliminar_Existente_EliminaYCommitea()
    {
        var mesa = new Mesa { CodigoCarrera = "ADM", NumeroMesa = 10 };
        _mesas.ObtenerAsync("ADM", 10, Arg.Any<CancellationToken>()).Returns(mesa);

        var resultado = await EliminarHandler().HandleAsync("ADM", 10, CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        _mesas.Received(1).Eliminar(mesa);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
