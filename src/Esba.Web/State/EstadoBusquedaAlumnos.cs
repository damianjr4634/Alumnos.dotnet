namespace Esba.Web.State;

/// <summary>
/// Estado del buscador global de alumnos (<c>Home.razor</c>) que sobrevive a la
/// navegación dentro del circuito: al entrar a la ficha del alumno u otras
/// pantallas y volver a la búsqueda, el texto, los filtros, la página y el
/// alumno seleccionado se restauran.
/// <para>
/// Registrado como <b>Scoped</b> → una instancia por circuito de Blazor Server
/// (equivalente a la sesión del usuario mientras dura la conexión). Se reinicia
/// con una recarga completa de la página o un login nuevo, no al navegar entre
/// pantallas.
/// </para>
/// </summary>
public sealed class EstadoBusquedaAlumnos
{
    /// <summary>Término del buscador: "apellido", "apellido:nombre", código o e-mail.</summary>
    public string? Texto { get; set; }

    public bool BuscarBajas { get; set; }

    public bool CarrerasEnDesuso { get; set; }

    /// <summary>Página actual de la grilla (0-based), para volver donde estaba.</summary>
    public int Pagina { get; set; }

    /// <summary>Código del último alumno seleccionado, para re-marcar la fila al volver.</summary>
    public string? CodigoSeleccionado { get; set; }

    /// <summary>Carrera del último alumno seleccionado (el código no es único entre carreras).</summary>
    public string? CarreraSeleccionada { get; set; }
}
