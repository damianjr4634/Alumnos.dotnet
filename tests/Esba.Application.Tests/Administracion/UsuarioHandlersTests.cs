using Esba.Application.Abstractions;
using Esba.Application.DTOs.Administracion;
using Esba.Application.Features.Administracion;
using Esba.Application.Validators;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using NSubstitute;

namespace Esba.Application.Tests.Administracion;

public class UsuarioHandlersTests
{
    private readonly IUsuarioRepository _usuarios = Substitute.For<IUsuarioRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ILegacyPasswordCipher _cipher = Substitute.For<ILegacyPasswordCipher>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private CrearUsuarioHandler CrearHandler() =>
        new(_usuarios, _hasher, _cipher, new CrearUsuarioValidator(), _unitOfWork);

    private ActualizarUsuarioHandler ActualizarHandler() =>
        new(_usuarios, new ActualizarUsuarioValidator(), _unitOfWork);

    private static CrearUsuarioCommand ComandoAlta() => new()
    {
        NombreUsuario = "jperez",
        Password = "clave123",
        Nombres = "Juan",
        Apellido = "Pérez",
        Cargo = "Bedel",
        EsSupervisor = false,
    };

    [Fact]
    public async Task Crear_UsuarioNuevo_HasheaAgregaYCommiteaUnaVez()
    {
        _usuarios.ExisteNombreAsync("JPEREZ", null, Arg.Any<CancellationToken>()).Returns(false);
        _hasher.Hash("clave123").Returns("$E1$hash");
        _cipher.Cifrar("clave123").Returns("cifradoLegacy");
        Usuario? capturado = null;
        _usuarios.When(u => u.Agregar(Arg.Any<Usuario>())).Do(ci => capturado = ci.Arg<Usuario>());

        var resultado = await CrearHandler().HandleAsync(ComandoAlta(), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        _usuarios.Received(1).Agregar(Arg.Any<Usuario>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal("$E1$hash", capturado!.PasswordHashNuevo);
        // PASSWD nace con el cifrado legacy: el usuario nuevo también puede entrar por el escritorio.
        Assert.Equal("cifradoLegacy", capturado.PasswordLegacy);
    }

    [Fact]
    public async Task Crear_NormalizaNombreAMayusculasYNaceConCambioForzadoYActivo()
    {
        _usuarios.ExisteNombreAsync(Arg.Any<string>(), null, Arg.Any<CancellationToken>()).Returns(false);
        _hasher.Hash(Arg.Any<string>()).Returns("$E1$hash");
        Usuario? capturado = null;
        _usuarios.When(u => u.Agregar(Arg.Any<Usuario>())).Do(ci => capturado = ci.Arg<Usuario>());

        await CrearHandler().HandleAsync(ComandoAlta() with { NombreUsuario = "  jperez  " }, CancellationToken.None);

        Assert.Equal("JPEREZ", capturado!.NombreUsuario);
        Assert.True(capturado.DebeCambiarPassword);
        Assert.Null(capturado.FechaBaja);
    }

    [Fact]
    public async Task Crear_NombreDuplicado_DevuelveErrorSinCommit()
    {
        _usuarios.ExisteNombreAsync("JPEREZ", null, Arg.Any<CancellationToken>()).Returns(true);

        var resultado = await CrearHandler().HandleAsync(ComandoAlta(), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        _usuarios.DidNotReceive().Agregar(Arg.Any<Usuario>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_PasswordCorta_DevuelveErrorSinTocarRepositorio()
    {
        var resultado = await CrearHandler().HandleAsync(
            ComandoAlta() with { Password = "ab" }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _usuarios.DidNotReceive().ExisteNombreAsync(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Actualizar_UsuarioInexistente_DevuelveErrorSinCommit()
    {
        _usuarios.ObtenerPorCodigoAsync(7, Arg.Any<CancellationToken>()).Returns((Usuario?)null);

        var resultado = await ActualizarHandler().HandleAsync(ComandoModif(), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Actualizar_NombreUsadoPorOtro_DevuelveErrorSinCommit()
    {
        _usuarios.ObtenerPorCodigoAsync(7, Arg.Any<CancellationToken>())
            .Returns(NuevoUsuario(7, esSupervisor: false));
        _usuarios.ExisteNombreAsync("JPEREZ", 7, Arg.Any<CancellationToken>()).Returns(true);

        var resultado = await ActualizarHandler().HandleAsync(ComandoModif(), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Actualizar_QuitaSupervisorAlUltimoSupervisor_DevuelveErrorSinCommit()
    {
        _usuarios.ObtenerPorCodigoAsync(7, Arg.Any<CancellationToken>())
            .Returns(NuevoUsuario(7, esSupervisor: true));
        _usuarios.ExisteNombreAsync(Arg.Any<string>(), 7, Arg.Any<CancellationToken>()).Returns(false);
        _usuarios.ContarSupervisoresActivosAsync(Arg.Any<CancellationToken>()).Returns(1);

        var resultado = await ActualizarHandler().HandleAsync(
            ComandoModif() with { EsSupervisor = false }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Actualizar_UsuarioExistente_AplicaCambiosYCommitea()
    {
        var existente = NuevoUsuario(7, esSupervisor: false);
        existente.Cargo = "Viejo";
        _usuarios.ObtenerPorCodigoAsync(7, Arg.Any<CancellationToken>()).Returns(existente);
        _usuarios.ExisteNombreAsync(Arg.Any<string>(), 7, Arg.Any<CancellationToken>()).Returns(false);

        var resultado = await ActualizarHandler().HandleAsync(
            ComandoModif() with { Cargo = "Secretario" }, CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal("Secretario", existente.Cargo);
        Assert.Equal("JPEREZ", existente.NombreUsuario);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static ActualizarUsuarioCommand ComandoModif() => new()
    {
        Codigo = 7,
        NombreUsuario = "jperez",
        Nombres = "Juan",
        Apellido = "Pérez",
        Cargo = "Bedel",
        EsSupervisor = false,
    };

    private static Usuario NuevoUsuario(int codigo, bool esSupervisor) => new()
    {
        Codigo = codigo,
        NombreUsuario = "JPEREZ",
        PasswordLegacy = "cifrado",
        EsSupervisor = esSupervisor,
    };
}
