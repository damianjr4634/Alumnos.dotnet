using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Application.Features.Academica;
using Esba.Application.Validators;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using NSubstitute;

namespace Esba.Application.Tests.Academica;

public class ComisionHandlersTests
{
    private readonly IComisionRepository _comisiones = Substitute.For<IComisionRepository>();
    private readonly IValidoComisionProcedure _validoComision = Substitute.For<IValidoComisionProcedure>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private CrearComisionHandler CrearHandler() =>
        new(_comisiones, _validoComision, new CrearComisionValidator());

    private ActualizarComisionHandler ActualizarHandler() =>
        new(_comisiones, new ActualizarComisionValidator());

    private EliminarComisionHandler EliminarHandler() =>
        new(_comisiones, _unitOfWork);

    private static CrearComisionCommand ComandoAlta() => new()
    {
        CodigoCarrera = "ADM",
        Cutuco = 111,
        CodigoMateria = "01",
        CuatrimestreAnio = "124",
        Horario = [new HorarioDiaComision { Dia = "LUNES", Primero = true, Segundo = true }],
    };

    private void SinDuplicado() =>
        _validoComision.VerificarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<short>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(true));

    [Fact]
    public async Task Crear_ComisionValida_GuardaYValida()
    {
        SinDuplicado();
        _comisiones.GuardarYValidarAsync(Arg.Any<Comision>(), true, Arg.Any<CancellationToken>())
            .Returns(Result.Ok("01"));

        var resultado = await CrearHandler().HandleAsync(ComandoAlta(), "tester", CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        await _comisiones.Received(1).GuardarYValidarAsync(Arg.Any<Comision>(), true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_MapeaHorarioAlFormatoLegacy()
    {
        SinDuplicado();
        Comision? capturada = null;
        _comisiones.GuardarYValidarAsync(Arg.Do<Comision>(c => capturada = c), true, Arg.Any<CancellationToken>())
            .Returns(Result.Ok("01"));

        await CrearHandler().HandleAsync(ComandoAlta(), "tester", CancellationToken.None);

        Assert.Equal("LUNES", capturada!.Dia1);
        Assert.Equal("PRISEG", capturada.Bloque1);   // 1º + 2º marcados
        Assert.Equal("BLANCO", capturada.Dia2);
        Assert.Equal("BLANCO", capturada.Bloque2);
        Assert.Equal("T", capturada.TitularSuplente);
        Assert.Equal("tester", capturada.Usuario);
    }

    [Fact]
    public async Task Crear_ComisionDuplicada_NoGuarda()
    {
        _validoComision.VerificarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<short>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Error<bool>("La comision ya existe para este cuatrimestre"));

        var resultado = await CrearHandler().HandleAsync(ComandoAlta(), "tester", CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _comisiones.DidNotReceive().GuardarYValidarAsync(Arg.Any<Comision>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_ComandoInvalido_NoChequeaDuplicadoNiGuarda()
    {
        var resultado = await CrearHandler().HandleAsync(
            ComandoAlta() with { CuatrimestreAnio = "1" }, "tester", CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _validoComision.DidNotReceive().VerificarAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<short>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _comisiones.DidNotReceive().GuardarYValidarAsync(Arg.Any<Comision>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Actualizar_ComisionInexistente_DevuelveError()
    {
        _comisiones.ObtenerAsync("ADM", (short)111, "01", "124", Arg.Any<CancellationToken>()).Returns((Comision?)null);

        var resultado = await ActualizarHandler().HandleAsync(new ActualizarComisionCommand
        {
            CodigoCarrera = "ADM",
            Cutuco = 111,
            CodigoMateria = "01",
            CuatrimestreAnio = "124",
        }, "tester", CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _comisiones.DidNotReceive().GuardarYValidarAsync(Arg.Any<Comision>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Actualizar_ComisionExistente_AplicaYGuarda()
    {
        var existente = new Comision { CodigoCarrera = "ADM", Cutuco = 111, CodigoMateria = "01", CuatrimestreAnio = "124" };
        _comisiones.ObtenerAsync("ADM", (short)111, "01", "124", Arg.Any<CancellationToken>()).Returns(existente);
        _comisiones.GuardarYValidarAsync(existente, false, Arg.Any<CancellationToken>()).Returns(Result.Ok("01"));

        var resultado = await ActualizarHandler().HandleAsync(new ActualizarComisionCommand
        {
            CodigoCarrera = "ADM",
            Cutuco = 111,
            CodigoMateria = "01",
            CuatrimestreAnio = "124",
            CodigoProfesor = "007",
            EsTitular = false,
        }, "tester", CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal("007", existente.CodigoProfesor);
        Assert.Equal("S", existente.TitularSuplente);
        await _comisiones.Received(1).GuardarYValidarAsync(existente, false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Eliminar_ComisionInexistente_NoCommitea()
    {
        _comisiones.ObtenerAsync("ADM", (short)111, "01", "124", Arg.Any<CancellationToken>()).Returns((Comision?)null);

        var resultado = await EliminarHandler().HandleAsync(new EliminarComisionCommand
        {
            CodigoCarrera = "ADM",
            Cutuco = 111,
            CodigoMateria = "01",
            CuatrimestreAnio = "124",
        }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        _comisiones.DidNotReceive().Eliminar(Arg.Any<Comision>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Eliminar_ComisionExistente_EliminaYCommitea()
    {
        var existente = new Comision { CodigoCarrera = "ADM", Cutuco = 111, CodigoMateria = "01", CuatrimestreAnio = "124" };
        _comisiones.ObtenerAsync("ADM", (short)111, "01", "124", Arg.Any<CancellationToken>()).Returns(existente);

        var resultado = await EliminarHandler().HandleAsync(new EliminarComisionCommand
        {
            CodigoCarrera = "ADM",
            Cutuco = 111,
            CodigoMateria = "01",
            CuatrimestreAnio = "124",
        }, CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        _comisiones.Received(1).Eliminar(existente);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
