using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Asistencias;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Queries;

/// <summary>
/// Lecturas Dapper de las carpetas por comisión. Reescritura parametrizada de los
/// SELECT concatenados de lstplanasis.pas y lstNotasyPractico.pas (ImprimirClick:
/// SqlComi y SqlDatos — ambos formularios usaban la misma nómina).
/// </summary>
/// <remarks>
/// Igual que en las actas (ActasQuery): el legacy comparaba <c>CUA_ANIO</c> sin barra
/// en la cabecera (COMARM) pero con barra en el detalle (CURSADA), inconsistencia que
/// solo funcionaba si el usuario tipeaba sin barra. Acá se normaliza quitando la barra
/// para ambas consultas. Otras dos diferencias deliberadas con el legacy: el truncado
/// de NOM_APE a 12 caracteres era por el ancho fijo de la hoja GDI (layout, no
/// negocio) y no se replica; y lstNotasyPractico.pas omitía el filtro
/// <c>A.BAJA='N'</c> en la impresión pero lo aplicaba en su export a Excel — se toma
/// como descuido y acá se filtra siempre.
/// </remarks>
public sealed class CarpetaComisionQuery : ICarpetaComisionQuery
{
    private readonly FbConnectionFactory _connectionFactory;

    public CarpetaComisionQuery(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    private static string NormalizarCuaAnio(string cuatrimestreAnio) =>
        (cuatrimestreAnio ?? string.Empty).Replace("/", string.Empty, StringComparison.Ordinal).Trim();

    private static void AgregarFiltrosOpcionales(
        List<string> filtros, DynamicParameters parametros, short? cutuco, string? codigoMateria)
    {
        if (cutuco.HasValue)
        {
            filtros.Add("C.CUTUCO = @Cutuco");
            parametros.Add("Cutuco", cutuco.Value);
        }

        if (!string.IsNullOrWhiteSpace(codigoMateria))
        {
            filtros.Add("C.COD_MAT = @CodMat");
            parametros.Add("CodMat", codigoMateria);
        }
    }

    public async Task<IReadOnlyList<CarpetaComisionCabeceraDto>> ObtenerComisionesAsync(
        string codigoCarrera,
        string cuatrimestreAnio,
        short? cutuco,
        string? codigoMateria,
        CancellationToken ct)
    {
        var parametros = new DynamicParameters();
        parametros.Add("Carre", codigoCarrera);
        parametros.Add("CuaAnio", NormalizarCuaAnio(cuatrimestreAnio));

        var filtros = new List<string> { "C.CARRE = @Carre", "C.CUA_ANIO = @CuaAnio" };
        AgregarFiltrosOpcionales(filtros, parametros, cutuco, codigoMateria);

        var sql = $"""
            SELECT C.CUTUCO          AS Cutuco,
                   TRIM(C.COD_MAT)   AS CodigoMateria,
                   TRIM(M.DESCRIPCI) AS DescripcionMateria,
                   TRIM(D.DOCENTE)   AS Docente,
                   TRIM(C.TIT_SUP)   AS TitularSuplente
            FROM COMARM C
            LEFT OUTER JOIN DOCENTES D ON C.CODPROFES = D.CODPROFES
            LEFT OUTER JOIN MATERIAS M ON C.COD_MAT = M.CODMATERI AND C.CARRE = M.CODCARRE
            WHERE {string.Join(" AND ", filtros)}
            ORDER BY C.CUTUCO, C.COD_MAT
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<CarpetaComisionCabeceraDto>(
            new CommandDefinition(sql, parametros, cancellationToken: ct)).ConfigureAwait(false);
        return filas.AsList();
    }

    public async Task<IReadOnlyList<CarpetaComisionAlumnoDto>> ObtenerAlumnosAsync(
        string codigoCarrera,
        string cuatrimestreAnio,
        short? cutuco,
        string? codigoMateria,
        CancellationToken ct)
    {
        var parametros = new DynamicParameters();
        parametros.Add("Carre", codigoCarrera);
        parametros.Add("CuaAnio", NormalizarCuaAnio(cuatrimestreAnio));

        var filtros = new List<string>
        {
            "C.CARRE = @Carre",
            "C.CUA_ANIO = @CuaAnio",
            "TRIM(C.CONDICION) IN ('CURSANDO', 'RECURSANDO')",
            "A.BAJA = 'N'",
        };
        AgregarFiltrosOpcionales(filtros, parametros, cutuco, codigoMateria);

        // Mismo ORDER BY que el legacy: la condición ordena CURSANDO antes que
        // RECURSANDO, que es lo que separaba las dos secciones de la hoja.
        var sql = $"""
            SELECT DISTINCT
                   TRIM(A.COD_ALU)   AS CodigoAlumno,
                   TRIM(A.APELLIDO)  AS Apellido,
                   TRIM(A.NOM_APE)   AS Nombre,
                   TRIM(C.CONDICION) AS Condicion,
                   C.CUTUCO          AS Cutuco,
                   TRIM(C.COD_MAT)   AS CodigoMateria
            FROM CURSADA C
            LEFT OUTER JOIN ALUMNOS A ON C.COD_ALU = A.COD_ALU AND C.CARRE = A.CARRE
            WHERE {string.Join(" AND ", filtros)}
            ORDER BY C.CUTUCO, TRIM(C.CONDICION), TRIM(A.APELLIDO), TRIM(A.NOM_APE)
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<CarpetaComisionAlumnoDto>(
            new CommandDefinition(sql, parametros, cancellationToken: ct)).ConfigureAwait(false);
        return filas.AsList();
    }
}
