using System.Globalization;
using Dapper;
using Esba.Domain.Examenes;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Queries;

namespace Esba.IntegrationTests.Queries;

/// <summary>
/// Equivalencia de ImpresionesMesasQuery contra el SQL legacy de Impresiones.pas
/// (Prompt 4.B), sobre los datos reales: docentes citados y sus mesas
/// (Imp_Mesas_citacion) y el parte diario (Imp_Mesas_ParteDiario).
/// </summary>
/// <remarks>
/// Diferencias deliberadas que el SELECT de referencia absorbe para poder comparar (ver
/// remarks de la query): se descartan las filas con docente NULL del LEFT JOIN legacy;
/// la hora se compara por su valor numérico y la materia sin el truncado a 30; el
/// parte diario se compara con COMI2 en lugar del COMI3 repetido y con los tres nombres
/// del tribunal sin truncar. Las comparaciones se hacen como conjuntos ordenados de
/// forma idéntica en C#, ya que el legacy ordenaba la hora como texto.
/// </remarks>
[Trait("Category", "Integration")]
public class ImpresionesMesasQueryEquivalenciaTests
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static FbConnectionFactory Factory => new(ConnectionString);

    private static string? Norm(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    /// <summary>El mes con más mesas con tribunal cargado, como rango de fechas de la muestra.</summary>
    private static async Task<(DateOnly Desde, DateOnly Hasta)> RangoMuestraAsync(System.Data.Common.DbConnection connection)
    {
        var muestra = await connection.QueryFirstOrDefaultAsync<(int Anio, int Mes)>("""
            SELECT FIRST 1 EXTRACT(YEAR FROM S.FECH_EXA) AS Anio, EXTRACT(MONTH FROM S.FECH_EXA) AS Mes
            FROM MESAS S
            JOIN DOCENTES D ON D.CODPROFES = S.TITULAR OR D.CODPROFES = S.VOCAL1 OR D.CODPROFES = S.VOCAL2
            WHERE S.FECH_EXA IS NOT NULL
            GROUP BY 1, 2
            ORDER BY COUNT(*) DESC
            """);
        Assert.True(muestra.Anio > 0, "Se necesita al menos una mesa con tribunal en MESAS para la prueba.");

        var desde = new DateOnly(muestra.Anio, muestra.Mes, 1);
        return (desde, desde.AddMonths(1).AddDays(-1));
    }

    [Fact]
    public async Task DocentesCitados_CoincidenConElSqlLegacy()
    {
        await using var connection = await Factory.CreateOpenConnectionAsync(CancellationToken.None);
        var (desde, hasta) = await RangoMuestraAsync(connection);

        var mios = (await new ImpresionesMesasQuery(Factory).ObtenerDocentesCitadosAsync(
                desde, hasta, null, null, [], CancellationToken.None))
            .Select(d => (d.CodigoProfesor, Norm(d.Docente)))
            .OrderBy(d => d.CodigoProfesor, StringComparer.Ordinal)
            .ToList();

        // SqlDatos de Imp_Mesas_citacion (sin rango de docentes ni carreras), descartando
        // el docente NULL que produce el LEFT JOIN para mesas sin tribunal.
        var referencia = (await connection.QueryAsync<(string? Docente, string? CodProfes)>(new CommandDefinition("""
            SELECT DISTINCT D.DOCENTE, D.CODPROFES
            FROM MESAS S
            LEFT OUTER JOIN DOCENTES D ON D.CODPROFES = S.TITULAR OR D.CODPROFES = S.VOCAL1 OR D.CODPROFES = S.VOCAL2
            WHERE S.FECH_EXA BETWEEN @Desde AND @Hasta
            ORDER BY 1
            """, new { Desde = desde, Hasta = hasta }, cancellationToken: CancellationToken.None)))
            .Where(r => r.CodProfes is not null)
            .Select(r => (r.CodProfes!.Trim(), Norm(r.Docente)))
            .OrderBy(d => d.Item1, StringComparer.Ordinal)
            .ToList();

        Assert.NotEmpty(referencia);
        Assert.Equal(referencia, mios);
    }

    [Fact]
    public async Task MesasDeLaCitacion_CoincidenConElSqlLegacy_PorDocente()
    {
        await using var connection = await Factory.CreateOpenConnectionAsync(CancellationToken.None);
        var (desde, hasta) = await RangoMuestraAsync(connection);

        var query = new ImpresionesMesasQuery(Factory);
        var docentes = await query.ObtenerDocentesCitadosAsync(desde, hasta, null, null, [], CancellationToken.None);
        Assert.NotEmpty(docentes);

        var mias = (await query.ObtenerMesasCitacionAsync(desde, hasta, null, null, [], CancellationToken.None))
            .Select(m => Clave(m.CodigoProfesor, m.FechaExamen, m.Hora, m.Materia, m.Mesa, m.Aula, m.CodigoCarrera))
            .OrderBy(k => k, StringComparer.Ordinal)
            .ToList();

        // SqlDatos2 de Imp_Mesas_citacion, ejecutado por docente como el legacy y unido.
        var referencia = new List<string>();
        foreach (var docente in docentes)
        {
            var filas = await connection.QueryAsync<(DateTime? FechExa, int? Hora, string? Sigla, int Mesa, int? Aula, string Carre)>(
                new CommandDefinition("""
                    SELECT S.FECH_EXA, S.HORA,
                           IIF(COALESCE(M.SIGLA, '') = '', M.DESCRIPCI, M.SIGLA) AS SIGLA,
                           S.MESA, S.AULA, S.CARRE
                    FROM MESAS S
                    LEFT OUTER JOIN MATERIAS M ON M.CODMATERI = S.COD_MAT AND M.CODCARRE = S.CARRE
                    LEFT OUTER JOIN DOCENTES D ON D.CODPROFES = S.TITULAR OR D.CODPROFES = S.VOCAL1 OR D.CODPROFES = S.VOCAL2
                    WHERE S.FECH_EXA BETWEEN @Desde AND @Hasta AND D.CODPROFES = @CodProfes
                    """, new { Desde = desde, Hasta = hasta, CodProfes = docente.CodigoProfesor }, cancellationToken: CancellationToken.None));

            referencia.AddRange(filas.Select(f => Clave(
                docente.CodigoProfesor, f.FechExa is null ? null : DateOnly.FromDateTime(f.FechExa.Value),
                f.Hora, f.Sigla, f.Mesa, f.Aula, f.Carre)));
        }

        referencia.Sort(StringComparer.Ordinal);
        Assert.NotEmpty(referencia);
        Assert.Equal(referencia, mias);
    }

    [Fact]
    public async Task ParteDiario_CoincideConElSqlLegacy()
    {
        await using var connection = await Factory.CreateOpenConnectionAsync(CancellationToken.None);
        var (desde, hasta) = await RangoMuestraAsync(connection);

        var mias = (await new ImpresionesMesasQuery(Factory).ObtenerParteDiarioAsync(desde, hasta, null, CancellationToken.None))
            .Select(m => string.Join("|",
                Iso(m.FechaExamen), m.CodigoCarrera, m.Mesa, m.Hora, Norm(m.Materia),
                FormatoMesa.Docentes(m.Titular, m.Vocal1, m.Vocal2),
                FormatoMesa.Comisiones(m.Comision1, m.Comision2, m.Comision3),
                m.CantidadAlumnos, m.Aula))
            .OrderBy(k => k, StringComparer.Ordinal)
            .ToList();

        // SqlDatos de Imp_Mesas_ParteDiario sin carrera ('' = todas), con COMI2 en vez del
        // COMI3 repetido y los nombres del tribunal sin truncar.
        var referencia = (await connection.QueryAsync<(string? Docente, DateTime? FechExa, int? Hora, string? Sigla, int Mesa, int? Aula, string Carre, long CanAlu, string Comision)>(
            new CommandDefinition("""
                SELECT TRIM(COALESCE(D1.DOCENTE, '')) || '-' || TRIM(COALESCE(D2.DOCENTE, '')) || '-' || TRIM(COALESCE(D3.DOCENTE, '')) AS DOCENTE,
                       S.FECH_EXA, S.HORA,
                       IIF(COALESCE(M.SIGLA, '') = '', M.DESCRIPCI, M.SIGLA) AS SIGLA,
                       S.MESA, S.AULA, S.CARRE,
                       (SELECT COUNT(*) FROM PERMEXA P WHERE P.CARRE = S.CARRE AND S.MESA = P.MESA AND P.FECH_EXA = S.FECH_EXA) AS CANALU,
                       COALESCE(S.COMI1, 0) || '/' || COALESCE(S.COMI2, 0) || '/' || COALESCE(S.COMI3, 0) AS COMISION
                FROM MESAS S
                LEFT OUTER JOIN MATERIAS M ON M.CODMATERI = S.COD_MAT AND M.CODCARRE = S.CARRE
                LEFT OUTER JOIN DOCENTES D1 ON D1.CODPROFES = S.TITULAR
                LEFT OUTER JOIN DOCENTES D2 ON D2.CODPROFES = S.VOCAL1
                LEFT OUTER JOIN DOCENTES D3 ON D3.CODPROFES = S.VOCAL2
                WHERE S.FECH_EXA BETWEEN @Desde AND @Hasta AND (S.CARRE = '' OR '' = '')
                ORDER BY S.FECH_EXA, S.CARRE, S.HORA
                """, new { Desde = desde, Hasta = hasta }, cancellationToken: CancellationToken.None)))
            .Select(r => string.Join("|",
                Iso(r.FechExa is null ? null : DateOnly.FromDateTime(r.FechExa.Value)), r.Carre.Trim(), r.Mesa, r.Hora, Norm(r.Sigla),
                DocentesLegacy(r.Docente), r.Comision, r.CanAlu, r.Aula))
            .OrderBy(k => k, StringComparer.Ordinal)
            .ToList();

        Assert.NotEmpty(referencia);
        Assert.Equal(referencia, mias);
    }

    /// <summary>"A-B-C" del legacy (con huecos para los ausentes) → los presentes unidos por " - ".</summary>
    private static string DocentesLegacy(string? docente) =>
        string.Join(" - ", (docente ?? string.Empty).Split('-').Select(d => d.Trim()).Where(d => d.Length > 0));

    private static string Clave(string profesor, DateOnly? fecha, int? hora, string? materia, int mesa, int? aula, string carrera) =>
        string.Join("|", profesor.Trim(), Iso(fecha), hora, Norm(materia), mesa, aula, carrera.Trim());

    private static string? Iso(DateOnly? fecha) => fecha?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
