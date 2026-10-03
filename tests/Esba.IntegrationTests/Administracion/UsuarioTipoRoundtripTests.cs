using Dapper;
using Esba.Application.DTOs.Administracion;
using Esba.Application.Features.Administracion;
using Esba.Application.Validators;
using Esba.Domain.Common;
using Esba.Domain.Enums;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Persistence.Repositories;
using Esba.Infrastructure.Queries;
using Esba.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace Esba.IntegrationTests.Administracion;

/// <summary>
/// Roundtrip de las columnas nuevas de USUARIOS (TIPO, CODPROFES, ALU_*; migración
/// 2026-10-02_usuarios_tipo_vinculo.sql) contra Firebird real: alta de un usuario
/// docente por el handler (EF, conversor TIPO ↔ enum), lectura por Dapper (CASE de
/// TIPO + join a DOCENTES), unicidad del vínculo y cambio de tipo. Crea su propio
/// docente de prueba y limpia siempre.
/// </summary>
[Trait("Category", "Integration")]
public class UsuarioTipoRoundtripTests
{
    private const string NombrePrueba = "ZZTIPODOC";
    private const string CodigoDocentePrueba = "Z98";

    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static DbContextOptions<EsbaDbContext> Opciones =>
        new DbContextOptionsBuilder<EsbaDbContext>().UseFirebird(ConnectionString).Options;

    private static EsbaDbContext CrearContexto() => new(Opciones);

    private static FbConnectionFactory Factory() => new(ConnectionString);

    private static async Task LimpiarAsync()
    {
        await using var ctx = CrearContexto();
        var cn = ctx.Database.GetDbConnection();
        await cn.ExecuteAsync("DELETE FROM USUARIOS WHERE NOMBRE = @N", new { N = NombrePrueba });
        await cn.ExecuteAsync("DELETE FROM DOCENTES WHERE CODPROFES = @C", new { C = CodigoDocentePrueba });
    }

    private static async Task CrearDocenteDePruebaAsync()
    {
        await using var ctx = CrearContexto();
        await ctx.Database.GetDbConnection().ExecuteAsync(
            "INSERT INTO DOCENTES (CODPROFES, DOCENTE) VALUES (@C, @D)",
            new { C = CodigoDocentePrueba, D = "Prueba, Roundtrip" });
    }

    private static CrearUsuarioHandler CrearHandler(EsbaDbContext ctx) => new(
        new UsuarioRepository(ctx), new DocenteRepository(ctx), new AlumnoRepository(ctx),
        new Pbkdf2PasswordHasher(), new EncriptoCadena2Cipher(), new CrearUsuarioValidator(), new EfUnitOfWork(ctx));

    private static ActualizarUsuarioHandler ActualizarHandler(EsbaDbContext ctx) => new(
        new UsuarioRepository(ctx), new DocenteRepository(ctx), new AlumnoRepository(ctx),
        new ActualizarUsuarioValidator(), new EfUnitOfWork(ctx));

    [Fact]
    public async Task AltaDocente_PersisteTipoYVinculo_LaGrillaLosLeeYElVinculoEsUnico()
    {
        var ct = CancellationToken.None;
        await LimpiarAsync();
        await CrearDocenteDePruebaAsync();

        try
        {
            int codigoUsuario;

            // Alta vía handler (EF): TIPO='DOC', CODPROFES, PASSWD con sentinela.
            await using (var ctx = CrearContexto())
            {
                var alta = await CrearHandler(ctx).HandleAsync(new CrearUsuarioCommand
                {
                    NombreUsuario = NombrePrueba,
                    Password = "clave123",
                    Apellido = "Prueba",
                    Tipo = TipoUsuario.Docente,
                    CodigoDocente = CodigoDocentePrueba,
                }, ct);

                Assert.Equal(OperationStatus.Ok, alta.Status);
                codigoUsuario = alta.Value;
            }

            // Lo que quedó físicamente en la fila.
            await using (var ctx = CrearContexto())
            {
                var fila = await ctx.Database.GetDbConnection().QuerySingleAsync<(string Tipo, string CodProfes, string Passwd)>(
                    "SELECT TRIM(TIPO), TRIM(CODPROFES), PASSWD FROM USUARIOS WHERE CODUSU = @C", new { C = codigoUsuario });
                Assert.Equal("DOC", fila.Tipo);
                Assert.Equal(CodigoDocentePrueba, fila.CodProfes);
                Assert.True(PasswordEscritorio.EstaBloqueado(fila.Passwd));
            }

            // Lectura EF: conversor TIPO ↔ enum y CHAR(3) con relleno.
            await using (var ctx = CrearContexto())
            {
                var usuario = await new UsuarioRepository(ctx).ObtenerPorNombreConPermisosAsync(NombrePrueba, ct);
                Assert.NotNull(usuario);
                Assert.Equal(TipoUsuario.Docente, usuario!.Tipo);
                Assert.Equal(CodigoDocentePrueba, usuario.CodigoDocente?.Trim());
                Assert.False(usuario.UsaEscritorio);
            }

            // Lectura Dapper (grilla): CASE de TIPO, join a DOCENTES y filtro por tipo.
            var pagina = await new UsuariosQuery(Factory()).BuscarAsync(
                new UsuariosFiltro { Texto = NombrePrueba, Tipo = TipoUsuario.Docente }, ct);
            var item = Assert.Single(pagina.Items);
            Assert.Equal(TipoUsuario.Docente, item.Tipo);
            Assert.Equal(CodigoDocentePrueba, item.CodigoDocente);
            Assert.Equal("Prueba, Roundtrip", item.NombreDocente);
            Assert.Equal($"{CodigoDocentePrueba} — Prueba, Roundtrip", item.Vinculo);

            var filtradoOtroTipo = await new UsuariosQuery(Factory()).BuscarAsync(
                new UsuariosFiltro { Texto = NombrePrueba, Tipo = TipoUsuario.Secretaria }, ct);
            Assert.Empty(filtradoOtroTipo.Items);

            // Unicidad del vínculo: un segundo usuario para el mismo docente se rechaza.
            await using (var ctx = CrearContexto())
            {
                var duplicado = await CrearHandler(ctx).HandleAsync(new CrearUsuarioCommand
                {
                    NombreUsuario = NombrePrueba + "2",
                    Password = "clave123",
                    Tipo = TipoUsuario.Docente,
                    CodigoDocente = CodigoDocentePrueba,
                }, ct);
                Assert.Equal(OperationStatus.Error, duplicado.Status);
            }

            // Cambio de tipo a secretaría: queda CAMPASS='S' y PASSWD sigue bloqueado (Warning).
            await using (var ctx = CrearContexto())
            {
                var cambio = await ActualizarHandler(ctx).HandleAsync(new ActualizarUsuarioCommand
                {
                    Codigo = codigoUsuario,
                    NombreUsuario = NombrePrueba,
                    Apellido = "Prueba",
                    Tipo = TipoUsuario.Secretaria,
                }, ct);
                Assert.Equal(OperationStatus.Warning, cambio.Status);
            }

            await using (var ctx = CrearContexto())
            {
                var fila = await ctx.Database.GetDbConnection().QuerySingleAsync<(string Tipo, string? CodProfes, string CamPass)>(
                    "SELECT TRIM(TIPO), CODPROFES, CAMPASS FROM USUARIOS WHERE CODUSU = @C", new { C = codigoUsuario });
                Assert.Equal("SEC", fila.Tipo);
                Assert.Null(fila.CodProfes);
                Assert.Equal("S", fila.CamPass);
            }
        }
        finally
        {
            await LimpiarAsync();
        }
    }
}
