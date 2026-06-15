using Esba.Application.Abstractions;
using Esba.Application.DTOs.Examenes;
using Esba.Application.Features.Examenes;
using Esba.Application.Validators;
using Esba.Domain.Common;
using NSubstitute;

namespace Esba.Application.Tests.Examenes;

public class PermisoExamenHandlersTests
{
    private readonly IPermisosExamenRepository _permisos = Substitute.For<IPermisosExamenRepository>();

    private CrearPermisoExamenHandler CrearHandler() => new(_permisos, new CrearPermisoExamenValidator());

    private EliminarPermisoExamenHandler EliminarHandler() => new(_permisos);

    private static CrearPermisoExamenCommand Valido() => new()
    {
        CodigoCarrera = "ADM",
        CodigoAlumno = "DNI30123456",
        Mesa = 10,
        Cutuco = 111,
        CodigoMateria = "01",
        CodigoUsuario = 7,
    };

    [Fact]
    public async Task Crear_NoExiste_Inserta()
    {
        _permisos.ExisteAsync("ADM", "DNI30123456", 10, "01", Arg.Any<CancellationToken>()).Returns(false);

        var resultado = await CrearHandler().HandleAsync(Valido(), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        await _permisos.Received(1).InsertarAsync(Arg.Any<CrearPermisoExamenCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_YaExiste_NoInserta()
    {
        _permisos.ExisteAsync("ADM", "DNI30123456", 10, "01", Arg.Any<CancellationToken>()).Returns(true);

        var resultado = await CrearHandler().HandleAsync(Valido(), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _permisos.DidNotReceive().InsertarAsync(Arg.Any<CrearPermisoExamenCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_Invalido_NoChequeaExistencia()
    {
        var resultado = await CrearHandler().HandleAsync(Valido() with { Mesa = 0 }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _permisos.DidNotReceive().ExisteAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Eliminar_SinFilas_DevuelveError()
    {
        _permisos.EliminarAsync("ADM", "DNI30123456", "01", Arg.Any<CancellationToken>()).Returns(0);

        var resultado = await EliminarHandler().HandleAsync("ADM", "DNI30123456", "01", CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
    }

    [Fact]
    public async Task Eliminar_ConFilas_DevuelveOk()
    {
        _permisos.EliminarAsync("ADM", "DNI30123456", "01", Arg.Any<CancellationToken>()).Returns(1);

        var resultado = await EliminarHandler().HandleAsync("ADM", "DNI30123456", "01", CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
    }
}
