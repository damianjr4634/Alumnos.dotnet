using Esba.Application.Abstractions;
using Esba.Application.DTOs.Administracion;
using Esba.Application.Features.Administracion;
using Esba.Application.Validators;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using NSubstitute;

namespace Esba.Application.Tests.Administracion;

public class AsignarPermisosUsuarioHandlerTests
{
    private readonly IUsuarioRepository _usuarios = Substitute.For<IUsuarioRepository>();
    private readonly ISeguGrabaProcedure _seguGraba = Substitute.For<ISeguGrabaProcedure>();

    private AsignarPermisosUsuarioHandler Handler() =>
        new(_usuarios, _seguGraba, new AsignarPermisosUsuarioValidator());

    private static AsignarPermisosUsuarioCommand Comando() => new()
    {
        CodigoUsuario = 7,
        CodigosOpcion = ["BAC", "001"],
    };

    private static Usuario Usuario(int codigo) => new()
    {
        Codigo = codigo,
        NombreUsuario = "U" + codigo,
        PasswordHash = "$E1$h",
    };

    [Fact]
    public async Task Asignar_UsuarioExistente_GrabaYDevuelveOk()
    {
        _usuarios.ObtenerPorCodigoAsync(7, Arg.Any<CancellationToken>()).Returns(Usuario(7));
        _seguGraba.GrabarAsync(7, Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(7));

        var resultado = await Handler().HandleAsync(Comando(), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        await _seguGraba.Received(1).GrabarAsync(7,
            Arg.Is<IReadOnlyList<string>>(l => l.Contains("BAC") && l.Contains("001")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Asignar_UsuarioInexistente_DevuelveErrorSinGrabar()
    {
        _usuarios.ObtenerPorCodigoAsync(7, Arg.Any<CancellationToken>()).Returns((Usuario?)null);

        var resultado = await Handler().HandleAsync(Comando(), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _seguGraba.DidNotReceive().GrabarAsync(Arg.Any<int>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Asignar_ComandoInvalido_DevuelveErrorSinTocarRepositorio()
    {
        var resultado = await Handler().HandleAsync(Comando() with { CodigoUsuario = 0 }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _usuarios.DidNotReceive().ObtenerPorCodigoAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _seguGraba.DidNotReceive().GrabarAsync(Arg.Any<int>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Asignar_ListaVacia_GrabaIgual()
    {
        _usuarios.ObtenerPorCodigoAsync(7, Arg.Any<CancellationToken>()).Returns(Usuario(7));
        _seguGraba.GrabarAsync(7, Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(7));

        var resultado = await Handler().HandleAsync(
            Comando() with { CodigosOpcion = [] }, CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        await _seguGraba.Received(1).GrabarAsync(7, Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>());
    }
}
