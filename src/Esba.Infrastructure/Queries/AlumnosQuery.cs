using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.Common;
using Esba.Application.DTOs.Alumnos;
using Esba.Domain.Entities;
using Esba.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Esba.Infrastructure.Queries;

/// <summary>
/// Padrón de alumnos. Reescritura parametrizada del SELECT que FrmEsba
/// (AbroBDDAlumnos, FrmEsba.pas:527-553) armaba concatenando .Text de
/// controles. La semántica de filtros se conserva 1:1; la paginación es nueva
/// (el legacy cargaba el padrón completo a un kbmMemTable en un thread).
/// Las columnas de XXX_OBSERV_PANTA (color/mensaje por alumno) NO van acá:
/// se exponen vía su wrapper tipado (Etapa 2.B) para la ficha rápida.
/// </summary>
public sealed class AlumnosQuery : IAlumnosQuery
{
    private const string ColumnasSelect = """
        SELECT TRIM(A.COD_ALU)  AS Codigo,
               TRIM(A.CARRE)    AS CodigoCarrera,
               TRIM(A.MATRIZ)   AS Matriz,
               TRIM(A.APELLIDO) AS Apellido,
               TRIM(A.NOM_APE)  AS Nombre,
               TRIM(A.MAIL)     AS Mail,
               IIF(A.BAJA = 'S', TRUE, FALSE) AS Baja,
               A.ESTADO         AS EstadoCodigo,
               TRIM(C.DESCARRE)   AS NombreCarrera,
               TRIM(C.RESOLUCION) AS Resolucion,
               IIF(C.DISTANCIA = 'S', 'DISTANCIA', 'PRESENCIAL') AS Modalidad
        """;

    private readonly FbConnectionFactory _connectionFactory;
    private readonly IDbContextFactory<EsbaDbContext> _contextFactory;

    public AlumnosQuery(FbConnectionFactory connectionFactory, IDbContextFactory<EsbaDbContext> contextFactory)
    {
        _connectionFactory = connectionFactory;
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// Detalle por EF Core en lugar de Dapper (justificación de la desviación
    /// del default 🟡 §1.3: reutiliza los conversores de los patrones CHAR(1)
    /// legacy — 'S'/'N', '*', enums por carácter — en vez de duplicarlos).
    /// </summary>
    public async Task<AlumnoDetailDto?> ObtenerDetalleAsync(string codigoCarrera, string codigo, CancellationToken ct)
    {
        await using var contexto = await _contextFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
        var alumno = await contexto.Alumnos.AsNoTracking()
            .FirstOrDefaultAsync(a => a.CodigoCarrera == codigoCarrera && a.Codigo == codigo, ct)
            .ConfigureAwait(false);

        return alumno is null ? null : MapearDetalle(alumno);
    }

    private static AlumnoDetailDto MapearDetalle(Alumno a) => new()
    {
        Codigo = a.Codigo.TrimEnd(),
        CodigoCarrera = a.CodigoCarrera.TrimEnd(),
        Matriz = a.Matriz?.TrimEnd(),
        Apellido = a.Apellido?.TrimEnd(),
        Nombre = a.Nombre?.TrimEnd(),
        DocumentoExpedidoPor = a.DocumentoExpedidoPor?.TrimEnd(),
        Sexo = a.Sexo,
        Nacionalidad = a.Nacionalidad?.TrimEnd(),
        EstadoCivil = a.EstadoCivil,
        FechaNacimiento = a.FechaNacimiento,
        LugarNacimiento = a.LugarNacimiento?.TrimEnd(),
        ProvinciaNacimiento = a.ProvinciaNacimiento?.TrimEnd(),
        Domicilio = a.Domicilio?.TrimEnd(),
        Localidad = a.Localidad?.TrimEnd(),
        CodigoPostal = a.CodigoPostal,
        CaracteristicaTelefono = a.CaracteristicaTelefono?.TrimEnd(),
        Telefono = a.Telefono?.TrimEnd(),
        Celular = a.Celular?.TrimEnd(),
        Mail = a.Mail?.TrimEnd(),
        FechaIngreso = a.FechaIngreso,
        ColegioPrimario = a.ColegioPrimario?.TrimEnd(),
        AnioPrimario = a.AnioPrimario?.TrimEnd(),
        TituloPrimario = a.TituloPrimario?.TrimEnd(),
        ColegioSecundario = a.ColegioSecundario?.TrimEnd(),
        AnioSecundario = a.AnioSecundario?.TrimEnd(),
        TituloSecundario = a.TituloSecundario?.TrimEnd(),
        InstitucionTerciaria = a.InstitucionTerciaria?.TrimEnd(),
        AnioTerciario = a.AnioTerciario?.TrimEnd(),
        TituloTerciario = a.TituloTerciario?.TrimEnd(),
        Empresa = a.Empresa?.TrimEnd(),
        Rubro = a.Rubro?.TrimEnd(),
        Cargo = a.Cargo?.TrimEnd(),
        Antiguedad = a.Antiguedad?.TrimEnd(),
        DomicilioLaboral = a.DomicilioLaboral?.TrimEnd(),
        TelefonoLaboral = a.TelefonoLaboral?.TrimEnd(),
        InternoLaboral = a.InternoLaboral?.TrimEnd(),
        TieneCertificadoEnTramite = a.TieneCertificadoEnTramite,
        FechaCertificadoEnTramite = a.FechaCertificadoEnTramite,
        SituacionDni = a.SituacionDni,
        PresentoCa = a.PresentoCa,
        Baja = a.Baja,
        Observaciones = a.Observaciones,
        PresentoFoto = a.PresentoFoto,
        PresentoAptoFisico = a.PresentoAptoFisico,
        FechaAptoFisico = a.FechaAptoFisico,
        UsuarioWeb = a.UsuarioWeb?.TrimEnd(),
        Foto = a.Foto,
        PresentoPartidaNacimiento = a.PresentoPartidaNacimiento,
        PresentoLibreta = a.PresentoLibreta,
        FechaLibreta = a.FechaLibreta,
        Estado = a.Estado,
        Genero = a.Genero,
        NominaPase = a.NominaPase,
    };

    public async Task<PagedResult<AlumnoListItemDto>> BuscarPadronAsync(PadronAlumnosFiltro filtro, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        if (!filtro.EsSupervisor && filtro.CodigoUsuario is null)
        {
            throw new InvalidOperationException("Un usuario no supervisor requiere CodigoUsuario para filtrar por BARRA_SEGU.");
        }

        var parametros = new DynamicParameters();
        var (from, where) = ArmarFromYWhere(filtro, parametros);

        var sqlItems = $"""
            {ColumnasSelect}
            {from}
            {where}
            ORDER BY C.DESCARRE, A.APELLIDO, A.NOM_APE, A.CARRE, A.COD_ALU
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
            """;
        parametros.Add("Skip", filtro.Skip);
        parametros.Add("Take", filtro.Take);

        var sqlTotal = $"SELECT COUNT(*) {from} {where}";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);

        var items = await connection.QueryAsync<AlumnoListItemDto>(
            new CommandDefinition(sqlItems, parametros, cancellationToken: ct)).ConfigureAwait(false);
        var total = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sqlTotal, parametros, cancellationToken: ct)).ConfigureAwait(false);

        return new PagedResult<AlumnoListItemDto> { Items = items.AsList(), Total = total };
    }

    /// <summary>
    /// Arma FROM y WHERE con la misma semántica del legacy. Solo se concatenan
    /// fragmentos SQL constantes: todo valor de usuario viaja como parámetro.
    /// </summary>
    private static (string From, string Where) ArmarFromYWhere(PadronAlumnosFiltro filtro, DynamicParameters parametros)
    {
        // Legacy: JOIN CARRERA ... + 'AND DESACT=''N''' salvo cbCarreDesuso.
        var from = "FROM ALUMNOS A JOIN CARRERA C ON C.CARRE = A.CARRE";
        if (!filtro.IncluirCarrerasEnDesuso)
        {
            from += " AND C.DESACT = 'N'";
        }

        // Legacy: si no es Superv, JOIN BARRA_SEGU por CodUsu.
        if (!filtro.EsSupervisor)
        {
            from += " JOIN BARRA_SEGU S ON S.BAROPC = A.CARRE AND S.CODUSU = @CodigoUsuario";
            parametros.Add("CodigoUsuario", filtro.CodigoUsuario);
        }

        var condiciones = new List<string>();

        if (!string.IsNullOrWhiteSpace(filtro.CodigoAlumno))
        {
            // Legacy: A.COD_ALU='<ACodAlu>' (búsqueda exacta).
            condiciones.Add("A.COD_ALU = @CodigoAlumno");
            parametros.Add("CodigoAlumno", filtro.CodigoAlumno);
        }
        else if (!string.IsNullOrWhiteSpace(filtro.CodigoCarrera))
        {
            // Legacy: atajo "_CARRE" del buscador.
            condiciones.Add("A.CARRE = @CodigoCarrera");
            parametros.Add("CodigoCarrera", filtro.CodigoCarrera.Trim());
        }
        else if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            condiciones.Add(ArmarBusquedaTexto(filtro.Texto, parametros));
        }

        // Legacy: chbBuscarBajas alterna entre solo bajas y solo activos.
        condiciones.Add("A.BAJA = @Baja");
        parametros.Add("Baja", filtro.BuscarDadosDeBaja ? "S" : "N");

        return (from, "WHERE " + string.Join(" AND ", condiciones));
    }

    /// <summary>
    /// Replica el parseo del TxtBusqueda legacy: "apellido[:nombre]", y el
    /// término matchea además mail (case-insensitive) y código de alumno.
    /// </summary>
    private static string ArmarBusquedaTexto(string texto, DynamicParameters parametros)
    {
        var partes = texto.Split(':', StringSplitOptions.TrimEntries);
        var apellido = partes[0];
        var nombre = partes.Length > 1 ? partes[1] : string.Empty;

        var porNombre = new List<string>();
        if (apellido.Length > 0)
        {
            porNombre.Add("A.APELLIDO CONTAINING @Apellido");
            parametros.Add("Apellido", apellido);
        }

        if (nombre.Length > 0)
        {
            porNombre.Add("A.NOM_APE CONTAINING @Nombre");
            parametros.Add("Nombre", nombre);
        }

        parametros.Add("TextoMayusculas", apellido.ToUpperInvariant());
        parametros.Add("Texto", apellido);
        var porMailOCodigo = "UPPER(A.MAIL) CONTAINING @TextoMayusculas OR A.COD_ALU CONTAINING @Texto";

        return porNombre.Count == 0
            ? $"({porMailOCodigo})"
            : $"(({string.Join(" AND ", porNombre)}) OR {porMailOCodigo})";
    }
}
