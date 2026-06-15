namespace Esba.Domain.Entities;

/// <summary>
/// Tabla MESA_TIPO: catálogo de tipos de mesa. PK: CODIGO. Aplica a una carrera
/// si <see cref="Carreras"/> contiene el "tipo" de la carrera (CARRERA.TIPO).
/// </summary>
public class TipoMesa
{
    /// <summary>CODIGO CHAR(2): código del tipo (PK).</summary>
    public required string Codigo { get; set; }

    /// <summary>DESCRI VARCHAR(30): descripción.</summary>
    public required string Descripcion { get; set; }

    /// <summary>CARRE VARCHAR(100): tipos de carrera a los que aplica.</summary>
    public string? Carreras { get; set; }
}
