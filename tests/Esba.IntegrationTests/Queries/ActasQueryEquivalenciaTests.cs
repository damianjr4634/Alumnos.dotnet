using Dapper;
using Esba.Application.DTOs.Examenes;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Queries;

namespace Esba.IntegrationTests.Queries;

/// <summary>
/// Equivalencia de ActasQuery contra el SQL legacy de las pantallas de actas
/// (Prompt 4.B). Compara, para los mismos parámetros y sobre los datos reales:
/// la cabecera por comisión (lstactasexamenes.pas, sin EXISTS) y la cabecera +
/// alumnos del acta volante de mesa (lstactasMesas.pas, vía XXX_MESAS_ALUMNOS).
/// El SELECT de referencia se materializa en los MISMOS DTOs que ActasQuery, de modo
/// que la comparación es de igualdad por valor de los registros.
/// </summary>
/// <remarks>
/// El legacy comparaba CUA_ANIO con/sin barra de forma inconsistente; el SELECT de
/// referencia usa el formato real de la columna CHAR(3) "124", igual que ActasQuery.
/// </remarks>
[Trait("Category", "Integration")]
public class ActasQueryEquivalenciaTests
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static FbConnectionFactory Factory => new(ConnectionString);

    [Fact]
    public async Task CabecerasComision_SinFiltroCondicion_CoincidenConElSqlLegacy()
    {
        await using var connection = await Factory.CreateOpenConnectionAsync(CancellationToken.None);

        var muestra = await connection.QueryFirstOrDefaultAsync<(string Carre, string CuaAnio)>("""
            SELECT FIRST 1 TRIM(CARRE) AS Carre, TRIM(CUA_ANIO) AS CuaAnio
            FROM COMARM
            WHERE COALESCE(TRIM(CUA_ANIO), '') <> ''
            """);

        Assert.True(muestra.Carre is not null, "Se necesita al menos una comisión en COMARM para la prueba.");

        var query = new ActasQuery(Factory);
        var mias = await query.ObtenerCabecerasComisionAsync(
            muestra.Carre, muestra.CuaAnio, null, null, ["CURSANDO", "RECURSANDO"], filtrarPorCondicion: false,
            CancellationToken.None);

        // SELECT de referencia: la cabecera de lstactasexamenes.pas (sin EXISTS).
        var referencia = (await connection.QueryAsync<ActaComisionCabeceraDto>(new CommandDefinition("""
            SELECT C.CUTUCO          AS Cutuco,
                   TRIM(C.COD_MAT)   AS CodigoMateria,
                   TRIM(M.DESCRIPCI) AS DescripcionMateria,
                   TRIM(D.DOCENTE)   AS Docente
            FROM COMARM C
            LEFT OUTER JOIN DOCENTES D ON C.CODPROFES = D.CODPROFES
            LEFT OUTER JOIN MATERIAS M ON C.COD_MAT = M.CODMATERI AND C.CARRE = M.CODCARRE
            WHERE C.CARRE = @Carre AND C.CUA_ANIO = @CuaAnio
            ORDER BY C.CUTUCO, C.COD_MAT
            """, new { muestra.Carre, muestra.CuaAnio }, cancellationToken: CancellationToken.None))).ToList();

        Assert.Equal(referencia, mias);
    }

    [Fact]
    public async Task ActaMesa_CabeceraYAlumnos_CoincidenConElSqlLegacy()
    {
        await using var connection = await Factory.CreateOpenConnectionAsync(CancellationToken.None);

        var muestra = await connection.QueryFirstOrDefaultAsync<(int Mesa, string Carre)>("""
            SELECT FIRST 1 M.MESA AS Mesa, TRIM(M.CARRE) AS Carre
            FROM MESAS M
            ORDER BY M.MESA
            """);

        Assert.True(muestra.Carre is not null, "Se necesita al menos una mesa en MESAS para la prueba.");

        var query = new ActasQuery(Factory);

        // Cabecera: misma fila que el SqlComi de lstactasMesas.pas.
        var miaCabecera = await query.ObtenerCabeceraMesaAsync(muestra.Mesa, muestra.Carre, CancellationToken.None);
        var refCabecera = await connection.QueryFirstOrDefaultAsync<ActaMesaCabeceraDto>(new CommandDefinition("""
            SELECT COALESCE(TRIM(T.DOCENTE), '')
                   || COALESCE(' - ' || TRIM(V1.DOCENTE), '')
                   || COALESCE(' - ' || TRIM(V2.DOCENTE), '')   AS Docente,
                   M.COMI1                                       AS Cutuco,
                   TRIM(MA.DESCRIPCI)                            AS DescripcionMateria,
                   EXTRACT(DAY   FROM M.FECH_EXA)                AS Dia,
                   EXTRACT(MONTH FROM M.FECH_EXA)                AS Mes,
                   EXTRACT(YEAR  FROM M.FECH_EXA)                AS Anio,
                   MA.CUATRIM                                    AS CuatrimestreMateria
            FROM MESAS M
            LEFT OUTER JOIN DOCENTES T  ON M.TITULAR = T.CODPROFES
            LEFT OUTER JOIN DOCENTES V1 ON M.VOCAL1  = V1.CODPROFES
            LEFT OUTER JOIN DOCENTES V2 ON M.VOCAL2  = V2.CODPROFES
            LEFT OUTER JOIN MATERIAS MA ON MA.CODMATERI = M.COD_MAT AND MA.CODCARRE = M.CARRE
            WHERE M.MESA = @Mesa AND M.CARRE = @Carre
            """, new { muestra.Mesa, muestra.Carre }, cancellationToken: CancellationToken.None));

        Assert.Equal(refCabecera, miaCabecera);

        // Alumnos: mismo conjunto que el SELECT directo a XXX_MESAS_ALUMNOS (tipo FINAL).
        var miosAlumnos = await query.ObtenerAlumnosMesaAsync(muestra.Mesa, muestra.Carre, "FINAL", CancellationToken.None);
        var refAlumnos = (await connection.QueryAsync<ActaAlumnoDto>(new CommandDefinition("""
            SELECT CAST(P.PERM_EXA AS INTEGER) AS PermisoExamen,
                   TRIM(P.COD_ALU)             AS CodigoAlumno,
                   TRIM(P.APELLIDO)            AS Apellido,
                   TRIM(P.NOM_APE)             AS Nombre
            FROM XXX_MESAS_ALUMNOS(@Mesa, @Carre, 'FINAL') P
            ORDER BY P.APELLIDO, P.NOM_APE
            """, new { muestra.Mesa, muestra.Carre }, cancellationToken: CancellationToken.None))).ToList();

        Assert.Equal(refAlumnos, miosAlumnos);
    }
}
