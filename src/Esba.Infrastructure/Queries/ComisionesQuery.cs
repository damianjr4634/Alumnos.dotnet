using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.Common;
using Esba.Application.DTOs.Academica;
using Esba.Domain.Academica;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Queries;

/// <summary>
/// Listado de comisiones armadas. Reescritura parametrizada del SELECT de
/// cargacomisiones.FormActivate (COMARM + LEFT JOIN MATERIAS + LEFT JOIN
/// DOCENTES). Paginación y orden server-side (§3.2).
/// </summary>
public sealed class ComisionesQuery : IComisionesQuery
{
    private const string ColumnasSelect = """
        SELECT TRIM(C.CARRE)     AS CodigoCarrera,
               C.CUTUCO          AS Cutuco,
               TRIM(C.COD_MAT)   AS CodigoMateria,
               TRIM(M.SIGLA)     AS SiglaMateria,
               TRIM(C.CUA_ANIO)  AS CuatrimestreAnio,
               TRIM(C.CODPROFES) AS CodigoProfesor,
               TRIM(D.DOCENTE)   AS Docente,
               TRIM(C.DIA1)      AS Dia1,
               TRIM(C.BLOQUE1)   AS Bloque1,
               TRIM(C.DIA2)      AS Dia2,
               TRIM(C.BLOQUE2)   AS Bloque2,
               TRIM(C.DIA3)      AS Dia3,
               TRIM(C.BLOQUE3)   AS Bloque3,
               TRIM(C.TIT_SUP)   AS TitularSuplente
        """;

    private const string FromYJoins = """
        FROM COMARM C
        LEFT OUTER JOIN DOCENTES D ON C.CODPROFES = D.CODPROFES
        LEFT OUTER JOIN MATERIAS M ON M.CODMATERI = C.COD_MAT AND M.CODCARRE = C.CARRE
        """;

    private const string OrdenDefecto = "C.CUTUCO, C.COD_MAT";

    private static readonly Dictionary<string, string> ColumnasOrdenables =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Cutuco"] = "C.CUTUCO",
            ["CodigoMateria"] = "C.COD_MAT",
            ["SiglaMateria"] = "M.SIGLA",
            ["Docente"] = "D.DOCENTE",
            ["CuatrimestreAnio"] = "C.CUA_ANIO",
        };

    private readonly FbConnectionFactory _connectionFactory;

    public ComisionesQuery(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PagedResult<ComisionListItemDto>> BuscarAsync(ComisionesFiltro filtro, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        var parametros = new DynamicParameters();
        var where = ArmarWhere(filtro, parametros);
        var orderBy = ArmarOrderBy(filtro);

        var sqlItems = $"""
            {ColumnasSelect}
            {FromYJoins}
            {where}
            ORDER BY {orderBy}
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
            """;
        parametros.Add("Skip", filtro.Skip);
        parametros.Add("Take", filtro.Take);

        var sqlTotal = $"SELECT COUNT(*) {FromYJoins} {where}";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);

        var items = await connection.QueryAsync<ComisionListItemDto>(
            new CommandDefinition(sqlItems, parametros, cancellationToken: ct)).ConfigureAwait(false);
        var total = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sqlTotal, parametros, cancellationToken: ct)).ConfigureAwait(false);

        return new PagedResult<ComisionListItemDto> { Items = items.AsList(), Total = total };
    }

    private static string ArmarWhere(ComisionesFiltro filtro, DynamicParameters parametros)
    {
        var condiciones = new List<string> { "C.CARRE = @Carre" };
        parametros.Add("Carre", filtro.CodigoCarrera);

        if (!string.IsNullOrWhiteSpace(filtro.CuatrimestreAnio))
        {
            condiciones.Add("C.CUA_ANIO = @CuaAnio");
            parametros.Add("CuaAnio", filtro.CuatrimestreAnio.Trim());
        }

        if (!string.IsNullOrWhiteSpace(filtro.CodigoMateria))
        {
            condiciones.Add("C.COD_MAT = @Mat");
            parametros.Add("Mat", filtro.CodigoMateria.Trim());
        }

        if (!string.IsNullOrWhiteSpace(filtro.CodigoProfesor))
        {
            condiciones.Add("C.CODPROFES = @Prof");
            parametros.Add("Prof", filtro.CodigoProfesor.Trim());
        }

        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            condiciones.Add("(M.SIGLA CONTAINING @Texto OR M.DESCRIPCI CONTAINING @Texto OR D.DOCENTE CONTAINING @Texto)");
            parametros.Add("Texto", filtro.Texto.Trim());
        }

        return "WHERE " + string.Join(" AND ", condiciones);
    }

    public async Task<ComisionDetailDto?> ObtenerDetalleAsync(
        string codigoCarrera, short cutuco, string codigoMateria, string cuatrimestreAnio, CancellationToken ct)
    {
        var sql = $"""
            {ColumnasSelect}
            {FromYJoins}
            WHERE C.CARRE = @Carre AND C.CUTUCO = @Cutuco AND C.COD_MAT = @CodMat AND C.CUA_ANIO = @CuaAnio
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var fila = await connection.QueryFirstOrDefaultAsync<ComisionListItemDto>(new CommandDefinition(
            sql,
            new { Carre = codigoCarrera, Cutuco = cutuco, CodMat = codigoMateria, CuaAnio = cuatrimestreAnio },
            cancellationToken: ct)).ConfigureAwait(false);

        if (fila is null)
        {
            return null;
        }

        return new ComisionDetailDto
        {
            CodigoCarrera = fila.CodigoCarrera,
            Cutuco = fila.Cutuco,
            CodigoMateria = fila.CodigoMateria,
            SiglaMateria = fila.SiglaMateria,
            CuatrimestreAnio = fila.CuatrimestreAnio,
            CodigoProfesor = fila.CodigoProfesor,
            EsTitular = fila.TitularSuplente?.Trim() == "T",
            Horario = DecodificarHorario(fila),
        };
    }

    /// <summary>Reconstruye las marcas por día desde DIAn/BLOQUEn de la fila.</summary>
    private static List<HorarioDiaComision> DecodificarHorario(ComisionListItemDto fila)
    {
        var pares = new[]
        {
            (fila.Dia1, fila.Bloque1),
            (fila.Dia2, fila.Bloque2),
            (fila.Dia3, fila.Bloque3),
        };

        var marcas = new List<HorarioDiaComision>();
        foreach (var (dia, bloque) in pares)
        {
            if (BloqueHorario.EsBlanco(dia) || BloqueHorario.EsBlanco(bloque))
            {
                continue;
            }

            var (primero, segundo, tercero) = BloqueHorario.Decodificar(bloque);
            marcas.Add(new HorarioDiaComision
            {
                Dia = dia!.Trim(),
                Primero = primero,
                Segundo = segundo,
                Tercero = tercero,
            });
        }

        return marcas;
    }

    private static string ArmarOrderBy(ComisionesFiltro filtro)
    {
        if (filtro.OrdenarPor is not null && ColumnasOrdenables.TryGetValue(filtro.OrdenarPor, out var columna))
        {
            var direccion = filtro.OrdenDescendente ? "DESC" : "ASC";
            return $"{columna} {direccion}, C.COD_MAT";
        }

        return OrdenDefecto;
    }

    public async Task<IReadOnlyList<AlumnoComisionCorreoDto>> ListarAlumnosDeComisionAsync(
        string codigoCarrera, short cutuco, string cuatrimestreAnio, CancellationToken ct)
    {
        // SELECT de buscarClick de enviocorreo.pas: alumnos activos CURSANDO/RECURSANDO
        // de la comisión (CUTUCO) en el cuatrimestre, con su mail. DISTINCT por alumno.
        const string sql = """
            SELECT DISTINCT A.COD_ALU                                    AS CodigoAlumno,
                   TRIM(A.APELLIDO) || ', ' || TRIM(A.NOM_APE)           AS NombreCompleto,
                   TRIM(A.MAIL)                                          AS Mail
            FROM CURSADA C
            LEFT OUTER JOIN ALUMNOS A ON A.COD_ALU = C.COD_ALU AND A.CARRE = C.CARRE
            WHERE C.CUTUCO = @Cutuco AND C.CUA_ANIO = @CuaAnio
              AND TRIM(C.CONDICION) IN ('CURSANDO', 'RECURSANDO')
              AND A.CARRE = @Carre AND A.BAJA = 'N'
            ORDER BY 2
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var items = await connection.QueryAsync<AlumnoComisionCorreoDto>(new CommandDefinition(
            sql,
            new { Cutuco = cutuco, CuaAnio = cuatrimestreAnio, Carre = codigoCarrera },
            cancellationToken: ct)).ConfigureAwait(false);

        return items.AsList();
    }
}
