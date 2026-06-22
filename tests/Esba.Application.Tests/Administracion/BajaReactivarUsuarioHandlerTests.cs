using Esba.Application.Abstractions;
using Esba.Application.Features.Administracion;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using NSubstitute;

namespace Esba.Application.Tests.Administracion;

public class BajaReactivarUsuarioHandlerTests
{
    private readonly IUsuarioRepository _usuarios = Substitute.For<IUsuarioRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private DarDeBajaUsuarioHandler BajaHandler() =>
        new(_usuarios, _unitOfWork, TimeProvider.System);

    private ReactivarUsuarioHandler ReactivarHandler() =>
        new(_usuarios, _unitOfWork);

    private static Usuario Activo(int codigo, bool supervisor = false) => new()
    {
        Codigo = codigo,
        NombreUsuario = "U" + codigo,
        PasswordHash = "$E1$h",
        EsSupervisor = supervisor,
        FechaBaja = null,
    };

    [Fact]
    public async Task DarDeBaja_ASiMismo_DevuelveErrorSinTocarRepositorio()
    {
        var resultado = await BajaHandler().HandleAsync(codigo: 5, codigoEjecutor: 5, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _usuarios.DidNotReceive().ObtenerPorCodigoAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DarDeBaja_Inexistente_DevuelveError()
    {
        _usuarios.ObtenerPorCodigoAsync(9, Arg.Any<CancellationToken>()).Returns((Usuario?)null);

        var resultado = await BajaHandler().HandleAsync(9, 1, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DarDeBaja_YaDeBaja_DevuelveWarningSinCommit()
    {
        var u = Activo(9);
        u.FechaBaja = new DateOnly(2026, 1, 1);
        _usuarios.ObtenerPorCodigoAsync(9, Arg.Any<CancellationToken>()).Returns(u);

        var resultado = await BajaHandler().HandleAsync(9, 1, CancellationToken.None);

        Assert.Equal(OperationStatus.Warning, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DarDeBaja_UltimoSupervisorActivo_DevuelveErrorSinCommit()
    {
        _usuarios.ObtenerPorCodigoAsync(9, Arg.Any<CancellationToken>()).Returns(Activo(9, supervisor: true));
        _usuarios.ContarSupervisoresActivosAsync(Arg.Any<CancellationToken>()).Returns(1);

        var resultado = await BajaHandler().HandleAsync(9, 1, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DarDeBaja_UsuarioComun_SeteaFechaYCommitea()
    {
        var u = Activo(9);
        _usuarios.ObtenerPorCodigoAsync(9, Arg.Any<CancellationToken>()).Returns(u);

        var resultado = await BajaHandler().HandleAsync(9, 1, CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.NotNull(u.FechaBaja);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reactivar_DadoDeBaja_LimpiaFechaYCommitea()
    {
        var u = Activo(9);
        u.FechaBaja = new DateOnly(2026, 1, 1);
        _usuarios.ObtenerPorCodigoAsync(9, Arg.Any<CancellationToken>()).Returns(u);

        var resultado = await ReactivarHandler().HandleAsync(9, CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Null(u.FechaBaja);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reactivar_UsuarioActivo_DevuelveWarningSinCommit()
    {
        _usuarios.ObtenerPorCodigoAsync(9, Arg.Any<CancellationToken>()).Returns(Activo(9));

        var resultado = await ReactivarHandler().HandleAsync(9, CancellationToken.None);

        Assert.Equal(OperationStatus.Warning, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reactivar_Inexistente_DevuelveError()
    {
        _usuarios.ObtenerPorCodigoAsync(9, Arg.Any<CancellationToken>()).Returns((Usuario?)null);

        var resultado = await ReactivarHandler().HandleAsync(9, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
