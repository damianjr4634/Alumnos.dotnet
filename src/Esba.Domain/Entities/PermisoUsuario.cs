namespace Esba.Domain.Entities;

/// <summary>
/// Tabla BARRA_SEGU: opción/carrera habilitada para un usuario. PK compuesta
/// (CODUSU, BAROPC). BAROPC referencia códigos de CARRERA (así filtra el
/// padrón) y de BARRA_OPC (opciones de menú). // TODO-migrar BARRA_OPC
/// En el destino estos permisos alimentan las políticas de autorización
/// (la UI oculta, el servidor deniega — migration_improvements.md §2.7).
/// </summary>
public class PermisoUsuario
{
    /// <summary>CODUSU INTEGER, parte de la PK.</summary>
    public int CodigoUsuario { get; set; }

    /// <summary>BAROPC VARCHAR(6), parte de la PK: código de carrera u opción de menú.</summary>
    public required string CodigoOpcion { get; set; }

    public Usuario? Usuario { get; set; }
}
