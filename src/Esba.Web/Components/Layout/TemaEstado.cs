namespace Esba.Web.Components.Layout;

/// <summary>
/// Estado del tema claro/oscuro que <see cref="EsbaRootLayout"/> cascadea a los shells
/// (secretaría y docente) y al botón de alternancia. Es un record inmutable: el root
/// crea una instancia nueva en cada cambio, así los consumidores se re-renderizan.
/// </summary>
/// <param name="Oscuro">true si el modo oscuro está activo.</param>
/// <param name="Alternar">Invierte el modo (lo resuelve el root layout).</param>
public sealed record TemaEstado(bool Oscuro, Func<Task> Alternar);
