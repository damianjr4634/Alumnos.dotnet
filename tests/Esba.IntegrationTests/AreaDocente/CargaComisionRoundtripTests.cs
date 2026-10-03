using Dapper;
using Esba.Application.DTOs.AreaDocente;
using Esba.Application.Features.AreaDocente;
using Esba.Application.Validators;
using Esba.Domain.Common;
using Esba.Domain.Enums;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Persistence.Repositories;
using Esba.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;

namespace Esba.IntegrationTests.AreaDocente;

/// <summary>
/// Roundtrip de la precarga por comisión contra Firebird real (DOC_CARGA_COMISION(_DET),
/// migración 2026-10-03): elige una comisión real con alumnos cursando y titular, guarda
/// un borrador por el titular (EF), lo relee por Dapper (join a CURSADA + detalle),
/// finaliza, verifica que el titular ya no edita, reabre por secretaría y limpia (el
/// borrado de la cabecera cascadea al detalle).
/// </summary>
[Trait("Category", "Integration")]
public class CargaComisionRoundtripTests
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static DbContextOptions<EsbaDbContext> Opciones =>
        new DbContextOptionsBuilder<EsbaDbContext>().UseFirebird(ConnectionString).Options;

    private static EsbaDbContext CrearContexto() => new(Opciones);

    private static FbConnectionFactory Factory() => new(ConnectionString);

    private static GuardarCargaComisionHandler Guardar(EsbaDbContext ctx) => new(
        new CargaComisionDocenteRepository(ctx), new CargaComisionDocenteQuery(Factory()),
        new GuardarCargaComisionValidator(), new EfUnitOfWork(ctx), TimeProvider.System);

    [Fact]
    public async Task Guardar_Finalizar_Reabrir_PersistenYSeLeenPorDapper()
    {
        var ct = CancellationToken.None;

        // Comisión real con titular, alumnos cursando y sin precarga previa.
        (string Carre, short Cutuco, string CodMat, string CuaAnio, string Titular)? elegida;
        await using (var cn = await Factory().CreateOpenConnectionAsync(ct))
        {
            elegida = await cn.QuerySingleOrDefaultAsync<(string, short, string, string, string)?>("""
                SELECT FIRST 1 TRIM(C.CARRE), C.CUTUCO, TRIM(C.COD_MAT), TRIM(C.CUA_ANIO), TRIM(C.CODPROFES)
                FROM COMARM C
                WHERE C.CODPROFES IS NOT NULL
                  AND EXISTS (SELECT 1 FROM CURSADA U JOIN ALUMNOS A ON A.COD_ALU = U.COD_ALU AND A.CARRE = U.CARRE
                              WHERE U.CARRE = C.CARRE AND U.CUTUCO = C.CUTUCO AND U.COD_MAT = C.COD_MAT
                                AND U.CUA_ANIO = C.CUA_ANIO AND TRIM(U.CONDICION) IN ('CURSANDO','RECURSANDO') AND A.BAJA = 'N')
                  AND NOT EXISTS (SELECT 1 FROM DOC_CARGA_COMISION DC
                                  WHERE DC.CARRE = C.CARRE AND DC.CUTUCO = C.CUTUCO AND DC.COD_MAT = C.COD_MAT AND DC.CUA_ANIO = C.CUA_ANIO)
                ORDER BY C.CUA_ANIO DESC
                """);
        }

        Assert.NotNull(elegida); // la base de desarrollo tiene comisiones con alumnos
        var (carre, cutuco, codMat, cuaAnio, titular) = elegida!.Value;
        var clave = new ClaveComision { CodigoCarrera = carre, Cutuco = cutuco, CodigoMateria = codMat, CuatrimestreAnio = cuaAnio };
        var actorTitular = new ActorCargaDocente { CodigoUsuario = 1, CodigoDocente = titular };
        var secretaria = new ActorCargaDocente { CodigoUsuario = 1, EsSecretaria = true };
        int? cargaId = null;

        try
        {
            var antes = await new CargaComisionDocenteQuery(Factory()).ObtenerAsync(clave, ct);
            Assert.NotNull(antes);
            Assert.False(antes!.TieneCarga);
            Assert.NotEmpty(antes.Alumnos);
            var alumno = antes.Alumnos[0];

            // Guardar borrador por el titular.
            await using (var ctx = CrearContexto())
            {
                var guardado = await Guardar(ctx).HandleAsync(new GuardarCargaComisionCommand
                {
                    Comision = clave,
                    Actor = actorTitular,
                    Observaciones = "Roundtrip de prueba",
                    Filas =
                    [
                        new FilaCargaComisionInput
                        {
                            CodigoAlumno = alumno.CodigoAlumno, Evaluacion1 = 7.5m, Recuperatorio = 99m, NotaRegular = 6m,
                            TotalHoras = 64, Inasistencias = 6, Justificadas = 2, Observaciones = "ok",
                        },
                    ],
                }, ct);
                Assert.Equal(OperationStatus.Ok, guardado.Status);
                cargaId = guardado.Value;
            }

            // Relectura Dapper: cabecera + detalle + CHAR sin relleno.
            var despues = await new CargaComisionDocenteQuery(Factory()).ObtenerAsync(clave, ct);
            Assert.Equal(cargaId, despues!.CargaId);
            Assert.Equal(EstadoCargaDocente.Borrador, despues.Estado);
            Assert.Equal("Roundtrip de prueba", despues.ObservacionesCarga);
            Assert.Equal(titular, despues.CodigoDocenteTitular);
            var fila = despues.Alumnos.Single(a => a.CodigoAlumno == alumno.CodigoAlumno);
            Assert.True(fila.TieneDetalle);
            Assert.Equal(7.5m, fila.Evaluacion1);
            Assert.Equal(99m, fila.Recuperatorio);
            Assert.Equal(6m, fila.NotaRegular);
            Assert.Equal((short)64, fila.TotalHoras);
            Assert.Equal((short)6, fila.Inasistencias);
            Assert.Equal("ok", fila.Observaciones);
            Assert.Equal(alumno.CursadaIndice, fila.CursadaIndice);
            // CURSADA no se tocó: los valores "Cursada*" siguen siendo los de antes.
            Assert.Equal(alumno.CursadaEvaluacion1, fila.CursadaEvaluacion1);

            // Finalizar por el titular; después el titular ya no puede guardar.
            await using (var ctx = CrearContexto())
            {
                var fin = await new FinalizarCargaComisionHandler(new CargaComisionDocenteRepository(ctx), new EfUnitOfWork(ctx), TimeProvider.System)
                    .HandleAsync(new CambiarEstadoCargaComisionCommand { Comision = clave, Actor = actorTitular }, ct);
                Assert.Equal(OperationStatus.Ok, fin.Status);
            }

            await using (var ctx = CrearContexto())
            {
                var bloqueado = await Guardar(ctx).HandleAsync(new GuardarCargaComisionCommand { Comision = clave, Actor = actorTitular }, ct);
                Assert.Equal(OperationStatus.Error, bloqueado.Status);
            }

            var finalizada = await new CargaComisionDocenteQuery(Factory()).ObtenerAsync(clave, ct);
            Assert.Equal(EstadoCargaDocente.Finalizada, finalizada!.Estado);
            Assert.NotNull(finalizada.FechaFinalizacion);

            // Reabrir por secretaría.
            await using (var ctx = CrearContexto())
            {
                var reabierta = await new ReabrirCargaComisionHandler(new CargaComisionDocenteRepository(ctx), new EfUnitOfWork(ctx), TimeProvider.System)
                    .HandleAsync(new CambiarEstadoCargaComisionCommand { Comision = clave, Actor = secretaria }, ct);
                Assert.Equal(OperationStatus.Ok, reabierta.Status);
            }

            var reabiertaDto = await new CargaComisionDocenteQuery(Factory()).ObtenerAsync(clave, ct);
            Assert.Equal(EstadoCargaDocente.Borrador, reabiertaDto!.Estado);

            // El inicio docente ve el estado de la carga.
            var comisionesDocente = await new AreaDocenteQuery(Factory()).ListarComisionesAsync(titular, ct);
            var enInicio = comisionesDocente.Single(c => c.CodigoCarrera == carre && c.Cutuco == cutuco
                && c.CodigoMateria == codMat && c.CuatrimestreAnio == cuaAnio);
            Assert.Equal("BOR", enInicio.EstadoCarga);
        }
        finally
        {
            if (cargaId is not null)
            {
                await using var cn = await Factory().CreateOpenConnectionAsync(ct);
                await cn.ExecuteAsync("DELETE FROM DOC_CARGA_COMISION WHERE ID = @Id", new { Id = cargaId });
                var detalles = await cn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM DOC_CARGA_COMISION_DET WHERE CARGA_ID = @Id", new { Id = cargaId });
                Assert.Equal(0, detalles); // cascada
            }
        }
    }
}
