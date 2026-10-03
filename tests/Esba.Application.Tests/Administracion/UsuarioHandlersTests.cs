using Esba.Application.Abstractions;
using Esba.Application.DTOs.Administracion;
using Esba.Application.Features.Administracion;
using Esba.Application.Validators;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using Esba.Domain.Enums;
using NSubstitute;

namespace Esba.Application.Tests.Administracion;

public class UsuarioHandlersTests
{
    private readonly IUsuarioRepository _usuarios = Substitute.For<IUsuarioRepository>();
    private readonly IDocenteRepository _docentes = Substitute.For<IDocenteRepository>();
    private readonly IAlumnoRepository _alumnos = Substitute.For<IAlumnoRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ILegacyPasswordCipher _cipher = Substitute.For<ILegacyPasswordCipher>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private CrearUsuarioHandler CrearHandler() =>
        new(_usuarios, _docentes, _alumnos, _hasher, _cipher, new CrearUsuarioValidator(), _unitOfWork);

    private ActualizarUsuarioHandler ActualizarHandler() =>
        new(_usuarios, _docentes, _alumnos, new ActualizarUsuarioValidator(), _unitOfWork);

    private static CrearUsuarioCommand ComandoAlta() => new()
    {
        NombreUsuario = "jperez",
        Password = "clave123",
        Nombres = "Juan",
        Apellido = "Pérez",
        Cargo = "Bedel",
        EsSupervisor = false,
    };

    private static CrearUsuarioCommand ComandoAltaDocente() => ComandoAlta() with
    {
        Tipo = TipoUsuario.Docente,
        CodigoDocente = "017",
    };

    private static Docente DocenteActivo(string codigo = "017") => new() { Codigo = codigo, Nombre = "Pérez, Juan" };

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
        Assert.Equal(TipoUsuario.Secretaria, capturado.Tipo);
        Assert.Null(capturado.CodigoDocente);
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
    public async Task Crear_Docente_VinculaYBloqueaPasswdDelEscritorio()
    {
        _usuarios.ExisteNombreAsync("JPEREZ", null, Arg.Any<CancellationToken>()).Returns(false);
        _docentes.ObtenerPorCodigoAsync("017", Arg.Any<CancellationToken>()).Returns(DocenteActivo());
        _usuarios.ExisteVinculoDocenteAsync("017", null, Arg.Any<CancellationToken>()).Returns(false);
        _hasher.Hash("clave123").Returns("$E1$hash");
        Usuario? capturado = null;
        _usuarios.When(u => u.Agregar(Arg.Any<Usuario>())).Do(ci => capturado = ci.Arg<Usuario>());

        var resultado = await CrearHandler().HandleAsync(ComandoAltaDocente(), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal(TipoUsuario.Docente, capturado!.Tipo);
        Assert.Equal("017", capturado.CodigoDocente);
        Assert.Equal("$E1$hash", capturado.PasswordHashNuevo);
        // El Delphi no mira TIPO: PASSWD queda con el sentinela, nunca con el cifrado de la clave.
        Assert.True(PasswordEscritorio.EstaBloqueado(capturado.PasswordLegacy));
        _cipher.DidNotReceiveWithAnyArgs().Cifrar(default!);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_Docente_NormalizaCodigoYDescartaVinculoDeAlumno()
    {
        _usuarios.ExisteNombreAsync(Arg.Any<string>(), null, Arg.Any<CancellationToken>()).Returns(false);
        _docentes.ObtenerPorCodigoAsync("A17", Arg.Any<CancellationToken>()).Returns(DocenteActivo("A17"));
        _hasher.Hash(Arg.Any<string>()).Returns("$E1$hash");
        Usuario? capturado = null;
        _usuarios.When(u => u.Agregar(Arg.Any<Usuario>())).Do(ci => capturado = ci.Arg<Usuario>());

        await CrearHandler().HandleAsync(ComandoAltaDocente() with
        {
            CodigoDocente = "a17",
            AlumnoCarrera = "TER",
            AlumnoCodigo = "123",
        }, CancellationToken.None);

        Assert.Equal("A17", capturado!.CodigoDocente);
        Assert.Null(capturado.AlumnoCarrera);
        Assert.Null(capturado.AlumnoCodigo);
    }

    [Fact]
    public async Task Crear_DocenteInexistente_DevuelveErrorSinCommit()
    {
        _usuarios.ExisteNombreAsync(Arg.Any<string>(), null, Arg.Any<CancellationToken>()).Returns(false);
        _docentes.ObtenerPorCodigoAsync("017", Arg.Any<CancellationToken>()).Returns((Docente?)null);

        var resultado = await CrearHandler().HandleAsync(ComandoAltaDocente(), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Contains("No existe el docente", resultado.Message, StringComparison.Ordinal);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_DocenteDadoDeBaja_DevuelveErrorSinCommit()
    {
        _usuarios.ExisteNombreAsync(Arg.Any<string>(), null, Arg.Any<CancellationToken>()).Returns(false);
        var docente = DocenteActivo();
        docente.FechaBaja = new DateOnly(2025, 12, 31);
        _docentes.ObtenerPorCodigoAsync("017", Arg.Any<CancellationToken>()).Returns(docente);

        var resultado = await CrearHandler().HandleAsync(ComandoAltaDocente(), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Contains("dado de baja", resultado.Message, StringComparison.Ordinal);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_DocenteYaConUsuario_DevuelveErrorSinCommit()
    {
        _usuarios.ExisteNombreAsync(Arg.Any<string>(), null, Arg.Any<CancellationToken>()).Returns(false);
        _docentes.ObtenerPorCodigoAsync("017", Arg.Any<CancellationToken>()).Returns(DocenteActivo());
        _usuarios.ExisteVinculoDocenteAsync("017", null, Arg.Any<CancellationToken>()).Returns(true);

        var resultado = await CrearHandler().HandleAsync(ComandoAltaDocente(), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Contains("ya tiene un usuario", resultado.Message, StringComparison.Ordinal);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_DocenteSupervisor_FallaEnValidacionSinTocarRepositorios()
    {
        var resultado = await CrearHandler().HandleAsync(
            ComandoAltaDocente() with { EsSupervisor = true }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _docentes.DidNotReceiveWithAnyArgs().ObtenerPorCodigoAsync(default!, default);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_AlumnoInexistente_DevuelveErrorSinCommit()
    {
        _usuarios.ExisteNombreAsync(Arg.Any<string>(), null, Arg.Any<CancellationToken>()).Returns(false);
        _alumnos.ObtenerAsync("TER", "12345678", Arg.Any<CancellationToken>()).Returns((Alumno?)null);

        var resultado = await CrearHandler().HandleAsync(ComandoAlta() with
        {
            Tipo = TipoUsuario.Alumno,
            AlumnoCarrera = "ter",
            AlumnoCodigo = "12345678",
        }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Contains("No existe el alumno TER-12345678", resultado.Message, StringComparison.Ordinal);
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
        // Sigue siendo secretaría: PASSWD intacto.
        Assert.Equal("cifrado", existente.PasswordLegacy);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Actualizar_SecretariaPasaADocente_BloqueaPasswdDelEscritorio()
    {
        var existente = NuevoUsuario(7, esSupervisor: false);
        _usuarios.ObtenerPorCodigoAsync(7, Arg.Any<CancellationToken>()).Returns(existente);
        _usuarios.ExisteNombreAsync(Arg.Any<string>(), 7, Arg.Any<CancellationToken>()).Returns(false);
        _docentes.ObtenerPorCodigoAsync("017", Arg.Any<CancellationToken>()).Returns(DocenteActivo());
        _usuarios.ExisteVinculoDocenteAsync("017", 7, Arg.Any<CancellationToken>()).Returns(false);

        var resultado = await ActualizarHandler().HandleAsync(
            ComandoModif() with { Tipo = TipoUsuario.Docente, CodigoDocente = "017" }, CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal(TipoUsuario.Docente, existente.Tipo);
        Assert.Equal("017", existente.CodigoDocente);
        Assert.True(PasswordEscritorio.EstaBloqueado(existente.PasswordLegacy));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Actualizar_DocentePasaASecretaria_FuerzaCambioDePasswordYAvisa()
    {
        var existente = NuevoUsuario(7, esSupervisor: false);
        existente.Tipo = TipoUsuario.Docente;
        existente.CodigoDocente = "017";
        existente.PasswordLegacy = PasswordEscritorio.GenerarBloqueo();
        existente.DebeCambiarPassword = false;
        _usuarios.ObtenerPorCodigoAsync(7, Arg.Any<CancellationToken>()).Returns(existente);
        _usuarios.ExisteNombreAsync(Arg.Any<string>(), 7, Arg.Any<CancellationToken>()).Returns(false);

        var resultado = await ActualizarHandler().HandleAsync(
            ComandoModif() with { Tipo = TipoUsuario.Secretaria }, CancellationToken.None);

        // La contraseña real no se conoce: PASSWD sigue bloqueado hasta el próximo cambio.
        Assert.Equal(OperationStatus.Warning, resultado.Status);
        Assert.Equal(TipoUsuario.Secretaria, existente.Tipo);
        Assert.Null(existente.CodigoDocente);
        Assert.True(existente.DebeCambiarPassword);
        Assert.True(PasswordEscritorio.EstaBloqueado(existente.PasswordLegacy));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Actualizar_DocenteCambiaAOtroDocenteYaConUsuario_DevuelveErrorSinCommit()
    {
        var existente = NuevoUsuario(7, esSupervisor: false);
        existente.Tipo = TipoUsuario.Docente;
        existente.CodigoDocente = "017";
        _usuarios.ObtenerPorCodigoAsync(7, Arg.Any<CancellationToken>()).Returns(existente);
        _usuarios.ExisteNombreAsync(Arg.Any<string>(), 7, Arg.Any<CancellationToken>()).Returns(false);
        _docentes.ObtenerPorCodigoAsync("018", Arg.Any<CancellationToken>()).Returns(DocenteActivo("018"));
        _usuarios.ExisteVinculoDocenteAsync("018", 7, Arg.Any<CancellationToken>()).Returns(true);

        var resultado = await ActualizarHandler().HandleAsync(
            ComandoModif() with { Tipo = TipoUsuario.Docente, CodigoDocente = "018" }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Equal("017", existente.CodigoDocente);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
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
