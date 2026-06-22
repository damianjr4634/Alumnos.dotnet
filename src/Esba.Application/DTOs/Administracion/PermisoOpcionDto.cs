namespace Esba.Application.DTOs.Administracion;

/// <summary>
/// Una opción asignable a un usuario (carrera de CARRERA u opción de menú de
/// BARRA_OPC), con su estado actual. Sucesor de la fila CADENA/HABILITA que
/// devuelve YYY_SEGU_OPCIONES; el wrapper separa el "CODIGO-Descripción".
/// </summary>
public sealed record PermisoOpcionDto
{
    /// <summary>BAROPC: código de carrera u opción que se graba en BARRA_SEGU.</summary>
    public required string Codigo { get; init; }

    public required string Descripcion { get; init; }

    /// <summary>true si el usuario ya tiene esta opción habilitada.</summary>
    public required bool Habilitado { get; init; }
}
