using Esba.Application.Features.Administracion;
using Esba.Infrastructure;
using Esba.Web.Components;
using Esba.Web.Seguridad;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Login por POST nativo: en Blazor Server la cookie solo puede emitirse en una
// request HTTP real, no desde el circuito interactivo.
app.MapPost("/auth/login", async (
    HttpContext http,
    [FromForm] string usuario,
    [FromForm] string password,
    IniciarSesionHandler handler,
    CancellationToken ct) =>
{
    var resultado = await handler.HandleAsync(
        new IniciarSesionCommand { NombreUsuario = usuario, Password = password }, ct);

    if (!resultado.IsSuccess || resultado.Value is null)
    {
        return Results.Redirect($"/login?error={Uri.EscapeDataString(resultado.Message ?? "No se pudo iniciar sesión.")}");
    }

    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, EsbaClaims.CrearPrincipal(resultado.Value));

    // TODO-migrar: si DebeCambiarPassword, redirigir a /cambiar-password cuando exista ese caso de uso.
    return Results.Redirect("/");
});

app.MapPost("/auth/logout", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.Run();
