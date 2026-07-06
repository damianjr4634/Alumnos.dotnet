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
        new(_usuarios, _hasher, _cipher, new BlanquearPasswordValidator(), _unitOfWork);

    private static Usuario Usuario() => new()
    {
        Codigo = 7,
        NombreUsuario = "JPEREZ",
        PasswordLegacy = "cifradoLegacy",
        PasswordHashNuevo = "$E1$viejo",
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
    public async Task Cambiar_ActualCorrecta_ActualizaAmbasColumnasQuitaCampassYCommitea()
    {
        var usuario = Usuario();
        _usuarios.ObtenerPorCodigoAsync(7, Arg.Any<CancellationToken>()).Returns(usuario);
        _hasher.Verify("$E1$viejo", "actual1").Returns(true);
        _hasher.Hash("nueva123").Returns("$E1$nuevo");
        _cipher.Cifrar("nueva123").Returns("cifradoNuevo");

        var resultado = await CambiarHandler().HandleAsync(Cambio(), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal("$E1$nuevo", usuario.PasswordHashNuevo);
        // PASSWD queda con el cifrado legacy: el escritorio Delphi sigue entrando.
        Assert.Equal("cifradoNuevo", usuario.PasswordLegacy);
        Assert.False(usuario.DebeCambiarPassword);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cambiar_SinNpasswdYActualEnFormatoLegacy_VerificaConCipherYCambia()
    {
        var usuario = Usuario();
        usuario.PasswordHashNuevo = null;
        usuario.PasswordLegacy = "legacy";
        _usuarios.ObtenerPorCodigoAsync(7, Arg.Any<CancellationToken>()).Returns(usuario);
        _hasher.CanVerify("legacy").Returns(false);
        _cipher.Descifrar("legacy").Returns("actual1");
        _hasher.Hash("nueva123").Returns("$E1$nuevo");
        _cipher.Cifrar("nueva123").Returns("cifradoNuevo");

        var resultado = await CambiarHandler().HandleAsync(Cambio(), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal("$E1$nuevo", usuario.PasswordHashNuevo);
        Assert.Equal("cifradoNuevo", usuario.PasswordLegacy);
    }

    [Fact]
    public async Task Cambiar_SinNpasswdYPasswdPisadoConHash_VerificaContraEseHashYRepara()
    {
        // Usuario cuyo PASSWD fue pisado con "$E1$" por la versión anterior del login.
        var usuario = Usuario();
        usuario.PasswordHashNuevo = null;
        usuario.PasswordLegacy = "$E1$pisado";
        _usuarios.ObtenerPorCodigoAsync(7, Arg.Any<CancellationToken>()).Returns(usuario);
        _hasher.CanVerify("$E1$pisado").Returns(true);
        _hasher.Verify("$E1$pisado", "actual1").Returns(true);
        _hasher.Hash("nueva123").Returns("$E1$nuevo");
        _cipher.Cifrar("nueva123").Returns("cifradoNuevo");

        var resultado = await CambiarHandler().HandleAsync(Cambio(), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal("$E1$nuevo", usuario.PasswordHashNuevo);
        Assert.Equal("cifradoNuevo", usuario.PasswordLegacy);
    }

    [Fact]
    public async Task Cambiar_ActualIncorrecta_DevuelveErrorSinCommit()
    {
        var usuario = Usuario();
        _usuarios.ObtenerPorCodigoAsync(7, Arg.Any<CancellationToken>()).Returns(usuario);
        _hasher.Verify("$E1$viejo", "actual1").Returns(false);

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
    public async Task Blanquear_UsuarioExistente_EscribeAmbasColumnasForzaCampassYCommitea()
    {
        var usuario = Usuario();
        usuario.DebeCambiarPassword = false;
        _usuarios.ObtenerPorCodigoAsync(7, Arg.Any<CancellationToken>()).Returns(usuario);
        _hasher.Hash("temporal1").Returns("$E1$temp");
        _cipher.Cifrar("temporal1").Returns("cifradoTemp");

        var resultado = await BlanquearHandler().HandleAsync(
            new BlanquearPasswordCommand { CodigoUsuario = 7, PasswordTemporal = "temporal1" }, CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal("$E1$temp", usuario.PasswordHashNuevo);
        Assert.Equal("cifradoTemp", usuario.PasswordLegacy);
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
