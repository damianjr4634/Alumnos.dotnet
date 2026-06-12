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

    private static Usuario UsuarioDePrueba(string passwd, bool debeCambiarPassword = false) => new()
    {
        Codigo = 7,
        NombreUsuario = "damian",
        PasswordHash = passwd,
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
    public async Task IniciarSesion_HashNuevoCorrecto_DevuelveOkYRegeneraSesionUnica()
    {
        var usuario = UsuarioDePrueba("$E1$hash");
        ConUsuarioEnRepositorio(usuario);
        _hasher.CanVerify("$E1$hash").Returns(true);
        _hasher.Verify("$E1$hash", "clave").Returns(true);

        var resultado = await CrearHandler().HandleAsync(ComandoValido(), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.NotNull(resultado.Value);
        Assert.Equal(7, resultado.Value.CodigoUsuario);
        Assert.True(resultado.Value.EsSupervisor);
        Assert.Contains("ADM", resultado.Value.Permisos);
        Assert.False(string.IsNullOrWhiteSpace(usuario.SesionUid));
        Assert.Equal(usuario.SesionUid, resultado.Value.SesionUid);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IniciarSesion_HashNuevoIncorrecto_DevuelveErrorYNoCommitea()
    {
        ConUsuarioEnRepositorio(UsuarioDePrueba("$E1$hash"));
        _hasher.CanVerify("$E1$hash").Returns(true);
        _hasher.Verify("$E1$hash", "clave").Returns(false);

        var resultado = await CrearHandler().HandleAsync(ComandoValido(), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task IniciarSesion_PasswordLegacyCorrecta_ReHasheaYCommitea()
    {
        var usuario = UsuarioDePrueba("cifradoLegacy");
        ConUsuarioEnRepositorio(usuario);
        _hasher.CanVerify("cifradoLegacy").Returns(false);
        _cipherLegacy.Descifrar("cifradoLegacy").Returns("clave");
        _hasher.Hash("clave").Returns("$E1$nuevo");

        var resultado = await CrearHandler().HandleAsync(ComandoValido(), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal("$E1$nuevo", usuario.PasswordHash);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IniciarSesion_PasswordLegacyIncorrecta_NoReHasheaNiCommitea()
    {
        var usuario = UsuarioDePrueba("cifradoLegacy");
        ConUsuarioEnRepositorio(usuario);
        _hasher.CanVerify("cifradoLegacy").Returns(false);
        _cipherLegacy.Descifrar("cifradoLegacy").Returns("otraClave");

        var resultado = await CrearHandler().HandleAsync(ComandoValido(), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Equal("cifradoLegacy", usuario.PasswordHash);
        _hasher.DidNotReceiveWithAnyArgs().Hash(default!);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task IniciarSesion_ConCamPass_IndicaDebeCambiarPassword()
    {
        var usuario = UsuarioDePrueba("$E1$hash", debeCambiarPassword: true);
        ConUsuarioEnRepositorio(usuario);
        _hasher.CanVerify("$E1$hash").Returns(true);
        _hasher.Verify("$E1$hash", "clave").Returns(true);

        var resultado = await CrearHandler().HandleAsync(ComandoValido(), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.True(resultado.Value!.DebeCambiarPassword);
    }
}
