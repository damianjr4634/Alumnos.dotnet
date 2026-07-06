using Esba.Application.Abstractions;
using Esba.Application.Features.Administracion;
using Esba.Application.Validators;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using NSubstitute;

namespace Esba.Application.Tests.Administracion;

public class IniciarSesionHandlerTests
{
    private readonly IUsuarioRepository _usuarios = Substitute.For<IUsuarioRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ILegacyPasswordCipher _cipherLegacy = Substitute.For<ILegacyPasswordCipher>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private IniciarSesionHandler CrearHandler() =>
        new(_usuarios, _hasher, _cipherLegacy, new IniciarSesionValidator(), _unitOfWork);

    private static IniciarSesionCommand ComandoValido(string password = "clave") =>
        new() { NombreUsuario = "damian", Password = password };

    private static Usuario UsuarioDePrueba(string passwd, string? npasswd = null, bool debeCambiarPassword = false) => new()
    {
        Codigo = 7,
        NombreUsuario = "damian",
        PasswordLegacy = passwd,
        PasswordHashNuevo = npasswd,
        Nombres = "Damián",
        Apellido = "García",
        EsSupervisor = true,
        DebeCambiarPassword = debeCambiarPassword,
        Permisos = [new PermisoUsuario { CodigoUsuario = 7, CodigoOpcion = "ADM" }],
    };

    private void ConUsuarioEnRepositorio(Usuario usuario) =>
        _usuarios.ObtenerPorNombreConPermisosAsync("damian", Arg.Any<CancellationToken>())
            .Returns(usuario);

    [Fact]
    public async Task IniciarSesion_ComandoInvalido_DevuelveErrorSinConsultarRepositorio()
    {
        var resultado = await CrearHandler().HandleAsync(
            new IniciarSesionCommand { NombreUsuario = "", Password = "" }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _usuarios.DidNotReceiveWithAnyArgs().ObtenerPorNombreConPermisosAsync(default!, default);
    }

    [Fact]
    public async Task IniciarSesion_UsuarioInexistente_DevuelveErrorGenericoYNoCommitea()
    {
        _usuarios.ObtenerPorNombreConPermisosAsync("damian", Arg.Any<CancellationToken>())
            .Returns((Usuario?)null);

        var resultado = await CrearHandler().HandleAsync(ComandoValido(), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Equal("Nombre de usuario o contraseña incorrectos.", resultado.Message);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task IniciarSesion_ConNpasswdCorrecto_DevuelveOkYRegeneraSesionUnica()
    {
        var usuario = UsuarioDePrueba("cifradoLegacy", npasswd: "$E1$hash");
        ConUsuarioEnRepositorio(usuario);
        _hasher.Verify("$E1$hash", "clave").Returns(true);

        var resultado = await CrearHandler().HandleAsync(ComandoValido(), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.NotNull(resultado.Value);
        Assert.Equal(7, resultado.Value.CodigoUsuario);
        Assert.True(resultado.Value.EsSupervisor);
        Assert.Contains("ADM", resultado.Value.Permisos);
        Assert.False(string.IsNullOrWhiteSpace(usuario.SesionUid));
        Assert.Equal(usuario.SesionUid, resultado.Value.SesionUid);
        // PASSWD (el del escritorio) no se toca cuando el usuario ya tiene NPASSWD.
        Assert.Equal("cifradoLegacy", usuario.PasswordLegacy);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IniciarSesion_ConNpasswdIncorrecto_DevuelveErrorSinMirarPasswd()
    {
        ConUsuarioEnRepositorio(UsuarioDePrueba("cifradoLegacy", npasswd: "$E1$hash"));
        _hasher.Verify("$E1$hash", "clave").Returns(false);

        var resultado = await CrearHandler().HandleAsync(ComandoValido(), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        // Con NPASSWD presente, PASSWD ya no participa del login web.
        _cipherLegacy.DidNotReceiveWithAnyArgs().Descifrar(default!);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task IniciarSesion_SinNpasswdYPasswordLegacyCorrecta_PueblaNpasswdSinRomperPasswd()
    {
        var usuario = UsuarioDePrueba("cifradoLegacy");
        ConUsuarioEnRepositorio(usuario);
        _hasher.CanVerify("cifradoLegacy").Returns(false);
        _cipherLegacy.Descifrar("cifradoLegacy").Returns("clave");
        _hasher.Hash("clave").Returns("$E1$nuevo");
        _cipherLegacy.Cifrar("clave").Returns("cifradoLegacy");

        var resultado = await CrearHandler().HandleAsync(ComandoValido(), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal("$E1$nuevo", usuario.PasswordHashNuevo);
        // PASSWD conserva el cifrado legacy: el escritorio sigue entrando.
        Assert.Equal("cifradoLegacy", usuario.PasswordLegacy);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IniciarSesion_SinNpasswdYPasswdPisadoConHash_ReparaPasswdParaElEscritorio()
    {
        // Usuario dañado por la versión anterior: PASSWD quedó con "$E1$" y el
        // escritorio no lo reconoce. Su próximo login web lo repara.
        var usuario = UsuarioDePrueba("$E1$pisado");
        ConUsuarioEnRepositorio(usuario);
        _hasher.CanVerify("$E1$pisado").Returns(true);
        _hasher.Verify("$E1$pisado", "clave").Returns(true);
        _hasher.Hash("clave").Returns("$E1$nuevo");
        _cipherLegacy.Cifrar("clave").Returns("cifradoReparado");

        var resultado = await CrearHandler().HandleAsync(ComandoValido(), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal("$E1$nuevo", usuario.PasswordHashNuevo);
        Assert.Equal("cifradoReparado", usuario.PasswordLegacy);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IniciarSesion_SinNpasswdYPasswordLegacyIncorrecta_NoEscribeNadaNiCommitea()
    {
        var usuario = UsuarioDePrueba("cifradoLegacy");
        ConUsuarioEnRepositorio(usuario);
        _hasher.CanVerify("cifradoLegacy").Returns(false);
        _cipherLegacy.Descifrar("cifradoLegacy").Returns("otraClave");

        var resultado = await CrearHandler().HandleAsync(ComandoValido(), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Null(usuario.PasswordHashNuevo);
        Assert.Equal("cifradoLegacy", usuario.PasswordLegacy);
        _hasher.DidNotReceiveWithAnyArgs().Hash(default!);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task IniciarSesion_ConCamPass_IndicaDebeCambiarPassword()
    {
        var usuario = UsuarioDePrueba("cifradoLegacy", npasswd: "$E1$hash", debeCambiarPassword: true);
        ConUsuarioEnRepositorio(usuario);
        _hasher.Verify("$E1$hash", "clave").Returns(true);

        var resultado = await CrearHandler().HandleAsync(ComandoValido(), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.True(resultado.Value!.DebeCambiarPassword);
    }
}
