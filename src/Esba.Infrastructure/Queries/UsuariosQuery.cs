using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.Common;
using Esba.Application.DTOs.Administracion;
using Esba.Domain.Enums;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Queries;

/// <summary>
/// Listado de usuarios del sistema, server-side (§3.2). Sucesor de la lectura de
/// AltaUsuario/BajaUsuarios. SUPERV/CAMPASS ('S'/'N') se proyectan a bool;
/// FECHA_BAJ distingue activos de dados de baja (baja lógica del hito 10.1a).
/// </summary>
public sealed class UsuariosQuery : IUsuariosQuery
{
    // TIPO se proyecta al valor numérico del enum TipoUsuario (SEC=0, DOC=1, ALU=2;
    // misma correspondencia que TipoUsuarioCodigo, que usa el mapeo EF). El nombre
    // del docente vinculado sale de DOCENTES para mostrarlo en la grilla.
    private const string ColumnasSelect = """
        SELECT U.CODUSU                                    AS Codigo,
               TRIM(U.NOMBRE)                              AS NombreUsuario,
               TRIM(U.NOMUSU)                              AS Nombres,
               TRIM(U.APELLIDO)                            AS Apellido,
               TRIM(U.CARGO)                               AS Cargo,
               CASE WHEN U.SUPERV = 'S' THEN 1 ELSE 0 END  AS EsSupervisor,
               CASE WHEN U.CAMPASS = 'S' THEN 1 ELSE 0 END AS DebeCambiarPassword,
               U.FECHA_BAJ                                 AS FechaBaja,
               CASE TRIM(U.TIPO) WHEN 'DOC' THEN 1 WHEN 'ALU' THEN 2 ELSE 0 END AS Tipo,
               TRIM(U.CODPROFES)                           AS CodigoDocente,
               TRIM(D.DOCENTE)                             AS NombreDocente,
               TRIM(U.ALU_CARRE)                           AS AlumnoCarrera,
               TRIM(U.ALU_COD_ALU)                         AS AlumnoCodigo
        """;

    private const string From = """
        FROM USUARIOS U
        LEFT JOIN DOCENTES D ON D.CODPROFES = U.CODPROFES
        """;

    private const string OrdenDefecto = "U.NOMBRE";

    private static readonly Dictionary<string, string> ColumnasOrdenables =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["NombreUsuario"] = "U.NOMBRE",
            ["Apellido"] = "U.APELLIDO",
            ["Cargo"] = "U.CARGO",
            ["Tipo"] = "U.TIPO",
        };

    private readonly FbConnectionFactory _connectionFactory;

    public UsuariosQuery(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PagedResult<UsuarioListItemDto>> BuscarAsync(UsuariosFiltro filtro, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        var parametros = new DynamicParameters();
        var where = ArmarWhere(filtro, parametros);
        var orderBy = ArmarOrderBy(filtro);

        var sqlItems = $"""
            {ColumnasSelect}
            {From}
            {where}
            ORDER BY {orderBy}
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
            """;
        parametros.Add("Skip", filtro.Skip);
        parametros.Add("Take", filtro.Take);

        var sqlTotal = $"SELECT COUNT(*) {From} {where}";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);

        var items = await connection.QueryAsync<UsuarioListItemDto>(
            new CommandDefinition(sqlItems, parametros, cancellationToken: ct)).ConfigureAwait(false);
        var total = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sqlTotal, parametros, cancellationToken: ct)).ConfigureAwait(false);

        return new PagedResult<UsuarioListItemDto> { Items = items.AsList(), Total = total };
    }

    private static string ArmarWhere(UsuariosFiltro filtro, DynamicParameters parametros)
    {
        var condiciones = new List<string>();

        if (!filtro.IncluirBajas)
        {
            condiciones.Add("U.FECHA_BAJ IS NULL");
        }

        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            condiciones.Add("(U.NOMBRE CONTAINING @Texto OR U.NOMUSU CONTAINING @Texto"
                + " OR U.APELLIDO CONTAINING @Texto OR U.CARGO CONTAINING @Texto"
                + " OR D.DOCENTE CONTAINING @Texto)");
            parametros.Add("Texto", filtro.Texto.Trim());
        }

        if (filtro.Tipo is { } tipo)
        {
            condiciones.Add("U.TIPO = @Tipo");
            parametros.Add("Tipo", TipoUsuarioCodigo.ACodigo(tipo));
        }

        return condiciones.Count == 0 ? string.Empty : "WHERE " + string.Join(" AND ", condiciones);
    }

    private static string ArmarOrderBy(UsuariosFiltro filtro)
    {
        if (filtro.OrdenarPor is not null && ColumnasOrdenables.TryGetValue(filtro.OrdenarPor, out var columna))
        {
            var direccion = filtro.Descendente ? "DESC" : "ASC";
            return $"{columna} {direccion}, NOMBRE";
        }

        return OrdenDefecto;
    }
}
