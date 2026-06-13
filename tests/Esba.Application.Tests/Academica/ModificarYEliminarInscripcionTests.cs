using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Application.Features.Academica;
using Esba.Application.Validators;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using NSubstitute;

namespace Esba.Application.Tests.Academica;

public class ModificarYEliminarInscripcionTests
{
    private readonly ICursadaRepository _cursadas = Substitute.For<ICursadaRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private static Cursada CursadaExistente() => new()
    {
        CodigoCarrera = "ADM",
        CodigoAlumno = "DNI30123456",
        CodigoMateria = "01",
        Cutuco = 912,
        CuatrimestreAnio = "125",
        Condicion = "CURSANDO",
    };

    private void ConCursadaExistente(Cursada cursada) =>
        _cursadas.ObtenerAsync("ADM", "DNI30123456", "01", Arg.Any<CancellationToken>())
            .Returns(cursada);

    [Fact]
    public async Task Modificar_InscripcionExistente_ActualizaYCommiteaUnaVez()
    {
        var cursada = CursadaExistente();
        ConCursadaExistente(cursada);
        var handler = new ModificarInscripcionHandler(_cursadas, new ModificarInscripcionValidator(), _unitOfWork);

        var resultado = await handler.HandleAsync(new ModificarInscripcionCommand
        {
            CodigoCarrera = "ADM",
            CodigoAlumno = "DNI30123456",
            CodigoMateria = "01",
            Turno = 3,
            NumeroComision = 4,
            CuatrimestreAnio = "226",
            Condicion = "RECURSANDO",
        }, "damian", CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal((short?)934, cursada.Cutuco);
        Assert.Equal("226", cursada.CuatrimestreAnio);
        Assert.Equal("RECURSANDO", cursada.Condicion);
        Assert.Equal("damian", cursada.Usuario);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Modificar_InscripcionInexistente_DevuelveErrorYNoCommitea()
    {
        var handler = new ModificarInscripcionHandler(_cursadas, new ModificarInscripcionValidator(), _unitOfWork);

        var resultado = await handler.HandleAsync(new ModificarInscripcionCommand
        {
            CodigoCarrera = "ADM",
            CodigoAlumno = "DNI30123456",
            CodigoMateria = "01",
            Turno = 1,
            NumeroComision = 1,
            CuatrimestreAnio = "126",
            Condicion = "CURSANDO",
        }, "damian", CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Equal("La inscripción no existe.", resultado.Message);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task Eliminar_InscripcionExistente_EliminaYCommiteaUnaVez()
    {
        var cursada = CursadaExistente();
        ConCursadaExistente(cursada);
        var handler = new EliminarInscripcionHandler(_cursadas, _unitOfWork);

        var resultado = await handler.HandleAsync(new EliminarInscripcionCommand
        {
            CodigoCarrera = "ADM",
            CodigoAlumno = "DNI30123456",
            CodigoMateria = "01",
        }, CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        _cursadas.Received(1).Eliminar(cursada);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Eliminar_InscripcionInexistente_DevuelveErrorYNoCommitea()
    {
        var handler = new EliminarInscripcionHandler(_cursadas, _unitOfWork);

        var resultado = await handler.HandleAsync(new EliminarInscripcionCommand
        {
            CodigoCarrera = "ADM",
            CodigoAlumno = "DNI30123456",
            CodigoMateria = "01",
        }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        _cursadas.DidNotReceiveWithAnyArgs().Eliminar(default!);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }
}
