using Esba.Application.Abstractions;
using Esba.Application.DTOs.Administracion;
using Esba.Application.Features.Administracion;
using Esba.Application.Validators;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using NSubstitute;

namespace Esba.Application.Tests.Administracion;

public class PasswordHandlersTests
{
    private readonly IUsuarioRepository _usuarios = Substitute.For<IUsuarioRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ILegacyPasswordCipher _cipher = Substitute.For<ILegacyPasswordCipher>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private CambiarPasswordHandler CambiarHandler() =>
        new(_usuarios, _hasher, _cipher, new CambiarPasswordValidator(), _unitOfWork);

    private BlanquearPasswordHandler BlanquearHandler() =>
        new(_usuarios, _hasher, new BlanquearPasswordValidator(), _unitOfWork);

    private static Usuario Usuario() => new()
    {
        Codigo = 7,
        NombreUsuario = "JPEREZ",
        PasswordHash = "$E1$viejo",
        DebeCambiarPassword = true,
    };

    private static CambiarPasswordCommand Cambio() => new()
    {
        CodigoUsuario = 7,
        PasswordActual = "actual1",
        PasswordNueva = "nueva123",
        PasswordNuevaConfirmacion = "nueva123",
    };

    [Fact]
    public async Task Cambiar_ActualCorrecta_HasheaQuitaCampassYCommitea()
    {
        var usuario = Usuario();
        _usuarios.ObtenerPorCodigoAsync(7, Arg.Any<CancellationToken>()).Returns(usuario);
        _hasher.CanVerify(usuario.PasswordHash).Returns(true);
        _hasher.Verify(usuario.PasswordHash, "actual1").Returns(true);
        _hasher.Hash("nueva123").Returns("$E1$nuevo");

        var resultado = await CambiarHandler().HandleAsync(Cambio(), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal("$E1$nuevo", usuario.PasswordHash);
        Assert.False(usuario.DebeCambiarPassword);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cambiar_ActualEnFormatoLegacy_VerificaConCipherYCambia()
    {
        var usuario = Usuario();
        usuario.PasswordHash = "legacy";
        _usuarios.ObtenerPorCodigoAsync(7, Arg.Any<CancellationToken>()).Returns(usuario);
        _hasher.CanVerify("legacy").Returns(false);
        _cipher.Descifrar("legacy").Returns("actual1");
        _hasher.Hash("nueva123").Returns("$E1$nuevo");

        var resultado = await CambiarHandler().HandleAsync(Cambio(), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal("$E1$nuevo", usuario.PasswordHash);
    }

    [Fact]
    public async Task Cambiar_ActualIncorrecta_DevuelveErrorSinCommit()
    {
        var usuario = Usuario();
        _usuarios.ObtenerPorCodigoAsync(7, Arg.Any<CancellationToken>()).Returns(usuario);
        _hasher.CanVerify(usuario.PasswordHash).Returns(true);
        _hasher.Verify(usuario.PasswordHash, "actual1").Returns(false);

        var resultado = await CambiarHandler().HandleAsync(Cambio(), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cambiar_UsuarioInexistente_DevuelveError()
    {
        _usuarios.ObtenerPorCodigoAsync(7, Arg.Any<CancellationToken>()).Returns((Usuario?)null);

        var resultado = await CambiarHandler().HandleAsync(Cambio(), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cambiar_ConfirmacionNoCoincide_DevuelveErrorSinTocarRepositorio()
    {
        var resultado = await CambiarHandler().HandleAsync(
            Cambio() with { PasswordNuevaConfirmacion = "otra" }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _usuarios.DidNotReceive().ObtenerPorCodigoAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Blanquear_UsuarioExistente_HasheaTemporalForzaCampassYCommitea()
    {
        var usuario = Usuario();
        usuario.DebeCambiarPassword = false;
        _usuarios.ObtenerPorCodigoAsync(7, Arg.Any<CancellationToken>()).Returns(usuario);
        _hasher.Hash("temporal1").Returns("$E1$temp");

        var resultado = await BlanquearHandler().HandleAsync(
            new BlanquearPasswordCommand { CodigoUsuario = 7, PasswordTemporal = "temporal1" }, CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal("$E1$temp", usuario.PasswordHash);
        Assert.True(usuario.DebeCambiarPassword);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Blanquear_UsuarioInexistente_DevuelveErrorSinCommit()
    {
        _usuarios.ObtenerPorCodigoAsync(7, Arg.Any<CancellationToken>()).Returns((Usuario?)null);

        var resultado = await BlanquearHandler().HandleAsync(
            new BlanquearPasswordCommand { CodigoUsuario = 7, PasswordTemporal = "temporal1" }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
