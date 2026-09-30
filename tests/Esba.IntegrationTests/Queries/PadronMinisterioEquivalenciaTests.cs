using Dapper;
using Esba.Application.DTOs.Ministerio;
using Esba.Application.Features.Ministerio;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Queries;

namespace Esba.IntegrationTests.Queries;

/// <summary>
/// Equivalencia de PadronMinisterioQuery + PadronMinisterioMapper contra el SQL legacy
/// de ComisionesAlMinisterio.pas (Prompt 4.B), sobre los datos reales:
/// <list type="bullet">
/// <item>la nómina impresa (SqlDatos de ImprimirClick): documento formateado por
/// posiciones y orden por comisión/condición/apellido;</item>
/// <item>el padrón Excel terciario (rama TER de BitBtn1Click) y secundario (rama
/// else), columna por columna.</item>
/// </list>
/// </summary>
/// <remarks>
/// Diferencias deliberadas que el SELECT de referencia absorbe para poder comparar:
/// CUA_ANIO siempre sin barra (el legacy la quitaba solo a veces); la división se
/// extiende a E/F (el CASE legacy cortaba en D, contradiciendo el mapa confirmado de
/// CodigoComision); los subselects de TUTORES llevan FIRST 1 (los escalares del legacy
/// fallaban con más de un tutor); '' y NULL se consideran iguales (el IIF de
/// ORIENTACION y la concatenación de TITULO devolvían '' donde acá va null). La edad
/// no se compara: el legacy la aproximaba con /365.
/// </remarks>
[Trait("Category", "Integration")]
public class PadronMinisterioEquivalenciaTests
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static FbConnectionFactory Factory => new(ConnectionString);

    /// <summary>La (carrera, cuatrimestre) con más cursadas que cumpla el filtro de carrera (SQL fijo, sin datos de usuario).</summary>
    private static string SqlMuestra(string filtroCarrera) => $"""
        SELECT FIRST 1 TRIM(C.CARRE) AS Carre, TRIM(C.CUA_ANIO) AS CuaAnio
        FROM CURSADA C
        JOIN CARRERA R ON R.CARRE = C.CARRE
        JOIN ALUMNOS A ON A.COD_ALU = C.COD_ALU AND A.CARRE = C.CARRE
        WHERE TRIM(C.CONDICION) IN ('CURSANDO', 'RECURSANDO') AND A.BAJA = 'N'
          AND {filtroCarrera}
        GROUP BY C.CARRE, C.CUA_ANIO
        ORDER BY COUNT(*) DESC
        """;

    private static string? Norm(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    private static string? Iso(DateOnly? fecha) =>
        fecha?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

    private static string? Iso(DateTime? fecha) =>
        fecha?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

    [Fact]
    public async Task NominaImpresa_CoincideConElSqlLegacy()
    {
        await using var connection = await Factory.CreateOpenConnectionAsync(CancellationToken.None);
        var muestra = await connection.QueryFirstOrDefaultAsync<(string Carre, string CuaAnio)>(SqlMuestra("1 = 1"));
        Assert.True(muestra.Carre is not null, "Se necesita al menos una cursada CURSANDO/RECURSANDO para la prueba.");

        var mios = (await new PadronMinisterioQuery(Factory).ObtenerAlumnosAsync(
                muestra.Carre, muestra.CuaAnio, null, CancellationToken.None))
            .Select(a =>
            {
                var n = PadronMinisterioMapper.AlumnoNomina(a, new DateOnly(2026, 9, 29));
                return (a.Cutuco, n.CodigoAlumno, Norm(n.Apellido), Norm(n.Nombre), Norm(a.Condicion), n.Documento, Norm(n.Nacionalidad));
            })
            .ToList();

        // SqlDatos de ImprimirClick, para todas las comisiones del cuatrimestre.
        var referencia = (await connection.QueryAsync<(short Cutuco, string CodAlu, string? Apellido, string? Nombre, string? Condicion, string Docum, string? Nacional)>(
            new CommandDefinition("""
                SELECT DISTINCT C.CUTUCO, A.COD_ALU, TRIM(A.APELLIDO) AS APELLIDO, TRIM(A.NOM_APE) AS NOM_APE,
                       TRIM(C.CONDICION) AS CONDICION,
                       SUBSTRING(A.COD_ALU FROM 1 FOR 3)||' '||SUBSTRING(A.COD_ALU FROM 4 FOR 2)||'.'||SUBSTRING(A.COD_ALU FROM 6 FOR 3)||'.'||SUBSTRING(A.COD_ALU FROM 9 FOR 3) AS DOCUM,
                       A.NACIONAL
                FROM CURSADA C
                LEFT OUTER JOIN ALUMNOS A ON C.COD_ALU = A.COD_ALU AND C.CARRE = A.CARRE
                WHERE C.CUA_ANIO = @CuaAnio
                  AND TRIM(C.CONDICION) IN ('CURSANDO', 'RECURSANDO') AND C.CARRE = @Carre AND A.BAJA = 'N'
                ORDER BY C.CUTUCO, C.CONDICION, A.APELLIDO, A.NOM_APE
                """, new { muestra.Carre, muestra.CuaAnio }, cancellationToken: CancellationToken.None)))
            .Select(r => (r.Cutuco, r.CodAlu.Trim(), Norm(r.Apellido), Norm(r.Nombre), Norm(r.Condicion), r.Docum, Norm(r.Nacional)))
            .ToList();

        Assert.NotEmpty(referencia);
        Assert.Equal(referencia, mios);
    }

    [Fact]
    public async Task PadronTerciario_CoincideConElSqlLegacy()
    {
        await CompararPadronAsync("R.TIPO = 'TER'", esTerciaria: true);
    }

    [Fact]
    public async Task PadronSecundario_CoincideConElSqlLegacy()
    {
        await CompararPadronAsync("R.TIPO <> 'TER'", esTerciaria: false);
    }

    /// <summary>Proyección común a ambos layouts: se comparan como tuplas de texto normalizado.</summary>
    private sealed record FilaReferencia
    {
        public string? Resolucion { get; init; }
        public string? Orientacion { get; init; }
        public string? TipoCarre { get; init; }
        public string? Modalidad { get; init; }
        public string? Turno { get; init; }
        public int? Anio { get; init; }
        public int? Cuat { get; init; }
        public string? Divi { get; init; }
        public string? Apellido { get; init; }
        public string? Nombre { get; init; }
        public string? TipDoc { get; init; }
        public string? Docum { get; init; }
        public string? Sex0 { get; init; }
        public DateTime? FecNac { get; init; }
        public string? Nacional { get; init; }
        public string? LugNac { get; init; }
        public string? Domi { get; init; }
        public int? CodPos { get; init; }
        public string? Locali { get; init; }
        public string? Cr { get; init; }
        public string? Titulo { get; init; }
        public string? ApellidoTutor { get; init; }
        public string? NombreTutor { get; init; }
        public string? TipDocTutor { get; init; }
        public int? DniTutor { get; init; }
        public string? NomHoja { get; init; }
        public short Cutuco { get; init; }
    }

    private static async Task CompararPadronAsync(string filtroCarrera, bool esTerciaria)
    {
        await using var connection = await Factory.CreateOpenConnectionAsync(CancellationToken.None);
        var muestra = await connection.QueryFirstOrDefaultAsync<(string Carre, string CuaAnio)>(SqlMuestra(filtroCarrera));
        Assert.True(muestra.Carre is not null, $"Se necesita al menos una cursada con {filtroCarrera} para la prueba.");

        var query = new PadronMinisterioQuery(Factory);
        var carrera = await query.ObtenerCarreraAsync(muestra.Carre, CancellationToken.None);
        Assert.NotNull(carrera);
        var mias = (await query.ObtenerAlumnosAsync(muestra.Carre, muestra.CuaAnio, null, CancellationToken.None))
            .Select(a => PadronMinisterioMapper.Fila(carrera!, a))
            .Select(Proyectar)
            .ToList();

        // Rama TER / rama else de BitBtn1Click (ver remarks de la clase por los ajustes).
        var columnasPropias = esTerciaria
            ? """
              IIF(UPPER(R.DESCARRE) STARTING 'TECNICATURA', 'TC', '') AS Orientacion,
              'G' AS TipoCarre,
              IIF(R.DISTANCIA = 'S', 'D', 'P') AS Modalidad,
              SUBSTRING(C.CUTUCO FROM 1 FOR 1) AS Cuat,
              IIF(TRIM(C.CONDICION) = 'RECURSANDO', 'RC', 'R') AS Cr,
              COALESCE(B.CSECU, '') || ' ' || COALESCE(B.TSECU, '') AS Titulo,
              NULL AS ApellidoTutor, NULL AS NombreTutor, NULL AS TipDocTutor, CAST(NULL AS INTEGER) AS DniTutor,
              CASE WHEN SUBSTRING(C.CUTUCO FROM 1 FOR 1) BETWEEN 1 AND 2 THEN 1
                   WHEN SUBSTRING(C.CUTUCO FROM 1 FOR 1) BETWEEN 3 AND 4 THEN 2
                   WHEN SUBSTRING(C.CUTUCO FROM 1 FOR 1) BETWEEN 5 AND 6 THEN 3 END AS Anio,
              """
            : """
              NULL AS Orientacion, NULL AS TipoCarre,
              IIF(R.TIPO = 'BAC', 'C', 'A') AS Modalidad,
              CAST(NULL AS INTEGER) AS Cuat, NULL AS Cr, NULL AS Titulo,
              (SELECT FIRST 1 T.FAPELLIDO FROM TUTORES T WHERE T.COD_ALU = C.COD_ALU AND T.CARRE = C.CARRE AND UPPER(T.FPARENT) IN ('PADRE','MADRE','TUTOR') ORDER BY T.FPARENT) AS ApellidoTutor,
              (SELECT FIRST 1 T.FNOMBRE FROM TUTORES T WHERE T.COD_ALU = C.COD_ALU AND T.CARRE = C.CARRE AND UPPER(T.FPARENT) IN ('PADRE','MADRE','TUTOR') ORDER BY T.FPARENT) AS NombreTutor,
              IIF(NOT EXISTS(SELECT 1 FROM TUTORES T WHERE T.COD_ALU = C.COD_ALU AND T.CARRE = C.CARRE AND UPPER(T.FPARENT) IN ('PADRE','MADRE','TUTOR')), NULL, 'DNI') AS TipDocTutor,
              (SELECT FIRST 1 T.FDNI FROM TUTORES T WHERE T.COD_ALU = C.COD_ALU AND T.CARRE = C.CARRE AND UPPER(T.FPARENT) IN ('PADRE','MADRE','TUTOR') ORDER BY T.FPARENT) AS DniTutor,
              CASE WHEN C.CARRE <> '650' THEN
                     CASE WHEN SUBSTRING(C.CUTUCO FROM 1 FOR 1) BETWEEN 1 AND 2 THEN 1
                          WHEN SUBSTRING(C.CUTUCO FROM 1 FOR 1) BETWEEN 3 AND 4 THEN 2
                          WHEN SUBSTRING(C.CUTUCO FROM 1 FOR 1) BETWEEN 5 AND 6 THEN 3 END
                   ELSE SUBSTRING(C.CUTUCO FROM 1 FOR 1) END AS Anio,
              """;

        var sqlReferencia = $"""
            SELECT DISTINCT R.RESOLUCION AS Resolucion,
                   {columnasPropias}
                   IIF(R.DISTANCIA = 'S', 'TD', CASE SUBSTRING(C.CUTUCO FROM 2 FOR 1) WHEN 1 THEN 'TM' WHEN 2 THEN 'TT' WHEN 3 THEN 'TV' WHEN 4 THEN 'TN' END) AS Turno,
                   CASE SUBSTRING(C.CUTUCO FROM 3 FOR 1) WHEN 1 THEN 'A' WHEN 2 THEN 'B' WHEN 3 THEN 'C' WHEN 4 THEN 'D' WHEN 5 THEN 'E' WHEN 6 THEN 'F' END AS Divi,
                   TRIM(B.APELLIDO) AS Apellido, TRIM(B.NOM_APE) AS Nombre,
                   SUBSTRING(B.COD_ALU FROM 1 FOR 3) AS TipDoc,
                   SUBSTRING(B.COD_ALU FROM 4 FOR 2)||'.'||SUBSTRING(B.COD_ALU FROM 6 FOR 3)||'.'||SUBSTRING(B.COD_ALU FROM 9 FOR 3) AS Docum,
                   IIF(B.SEXO = 'F', 'MUJER', 'VARON') AS Sex0, B.FEC_NAC AS FecNac, B.NACIONAL AS Nacional, B.LUG_NAC AS LugNac,
                   B.DOMI AS Domi, B.COD_POS AS CodPos, B.LOCALI AS Locali,
                   R.CARRE || '-' || C.CUTUCO AS NomHoja, C.CUTUCO AS Cutuco
            FROM CURSADA C
            LEFT OUTER JOIN CARRERA R ON R.CARRE = C.CARRE
            LEFT OUTER JOIN ALUMNOS B ON C.COD_ALU = B.COD_ALU AND C.CARRE = B.CARRE
            WHERE C.CUA_ANIO = @CuaAnio
              AND TRIM(C.CONDICION) IN ('CURSANDO', 'RECURSANDO') AND B.BAJA = 'N'
              AND C.CARRE = @Carre
            ORDER BY C.CARRE, C.CUTUCO, C.CONDICION, B.APELLIDO, B.NOM_APE
            """;

        var referencia = (await connection.QueryAsync<FilaReferencia>(new CommandDefinition(
                sqlReferencia, new { muestra.Carre, muestra.CuaAnio }, cancellationToken: CancellationToken.None)))
            .Select(Proyectar)
            .ToList();

        Assert.NotEmpty(referencia);
        Assert.Equal(referencia, mias);
    }

    private static string Proyectar(FilaPadronMinisterioDto f) => string.Join("|",
        Norm(f.Resolucion), Norm(f.Orientacion), Norm(f.TipoCarrera), Norm(f.Modalidad), Norm(f.Turno),
        f.AnioEstudio, f.Cuatrimestre, Norm(f.Division), Norm(f.Apellido), Norm(f.Nombre),
        Norm(f.TipoDocumento), Norm(f.NumeroDocumento), Norm(f.Genero), Iso(f.FechaNacimiento),
        Norm(f.PaisNacimiento), Norm(f.LugarNacimiento), Norm(f.Domicilio), f.CodigoPostal, Norm(f.Localidad),
        Norm(f.Condicion), Norm(f.TituloIngreso), Norm(f.ApellidoTutor), Norm(f.NombreTutor),
        Norm(f.TipoDocumentoTutor), f.DniTutor, Norm(f.NombreHoja), f.Cutuco);

    private static string Proyectar(FilaReferencia r) => string.Join("|",
        Norm(r.Resolucion), Norm(r.Orientacion), Norm(r.TipoCarre), Norm(r.Modalidad), Norm(r.Turno),
        r.Anio, r.Cuat, Norm(r.Divi), Norm(r.Apellido), Norm(r.Nombre),
        Norm(r.TipDoc), Norm(r.Docum), Norm(r.Sex0), Iso(r.FecNac),
        Norm(r.Nacional), Norm(r.LugNac), Norm(r.Domi), r.CodPos, Norm(r.Locali),
        Norm(r.Cr), Norm(r.Titulo), Norm(r.ApellidoTutor), Norm(r.NombreTutor),
        Norm(r.TipDocTutor), r.DniTutor, Norm(r.NomHoja), r.Cutuco);
}
