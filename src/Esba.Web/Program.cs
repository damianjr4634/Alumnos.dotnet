using Esba.Application.DTOs.Certificados;
using Esba.Application.Features.Administracion;
using Esba.Application.Features.Certificados;
using Esba.Domain.Enums;
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
builder.Services.AddAuthorization(options =>
{
    // Acceso a las pantallas de Administración: solo supervisores (SUPERV='S').
    // La autorización fina por área (mapa BARRA_OPC/BARRA_SEGU) es del hito 12.
    options.AddPolicy(EsbaPolicies.Supervisores, policy =>
        policy.RequireClaim(EsbaClaims.Supervisor, "S"));
});
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

    // CAMPASS='S': el usuario aterriza en el cambio de contraseña forzado (10.1c).
    // El bloqueo estricto de navegación hasta cambiarla es del hito 12.
    return Results.Redirect(resultado.Value.DebeCambiarPassword ? "/cambiar-password" : "/");
});

app.MapPost("/auth/logout", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

// Constancia de alumno (hito 9.1): sirve el PDF inline para previsualizar/imprimir
// en el navegador (sucesor de Imprimir.pas/TGmPreview, §3.3). El servidor es la
// autoridad: re-valida con el SP de chequeo aunque la página ya haya confirmado.
app.MapGet("/constancias/alumno", async (
    string carre,
    string cod,
    string tipo,
    string? ante,
    bool conf,
    bool membrete,
    GenerarConstanciaAlumnoHandler handler,
    CancellationToken ct) =>
{
    if (!Enum.TryParse<TipoConstancia>(tipo, ignoreCase: true, out var tipoConstancia))
    {
        return Results.BadRequest("Tipo de constancia inválido.");
    }

    var command = new GenerarConstanciaCommand
    {
        CodigoCarrera = carre,
        CodigoAlumno = cod,
        Tipo = tipoConstancia,
        AnteQuien = ante,
        IncluirMembrete = membrete,
    };

    var resultado = await handler.GenerarPdfAsync(command, conf, ct);
    if (!resultado.IsSuccess || resultado.Value is null)
    {
        return Results.BadRequest(resultado.Message ?? "No se pudo generar la constancia.");
    }

    return Results.File(resultado.Value, "application/pdf");
}).RequireAuthorization();

// Constancia de Materias Aprobadas (hito 9.2b): reporte tabular del analítico,
// servido inline para previsualizar/imprimir en el navegador (sucesor de
// BitBtn1Click de constanciaalumnos2.pas, §3.3).
app.MapGet("/constancias/alumno/materias-aprobadas", async (
    string carre,
    string cod,
    string? ante,
    bool membrete,
    GenerarConstanciaMateriasAprobadasHandler handler,
    CancellationToken ct) =>
{
    var resultado = await handler.GenerarPdfAsync(cod, carre, ante, membrete, ct);
    if (!resultado.IsSuccess || resultado.Value is null)
    {
        return Results.BadRequest(resultado.Message ?? "No se pudo generar la constancia.");
    }

    return Results.File(resultado.Value, "application/pdf");
}).RequireAuthorization();

// Constancia de Examen Final (hito 9.2c): se emite por materia (acción de fila del
// analítico). Servida inline (sucesor de Impresion_Constancia_Examen, §3.3).
app.MapGet("/constancias/alumno/examen-final", async (
    string carre,
    string cod,
    string codmat,
    string? ante,
    bool membrete,
    GenerarConstanciaExamenFinalHandler handler,
    CancellationToken ct) =>
{
    var resultado = await handler.GenerarPdfAsync(cod, carre, codmat, ante, membrete, ct);
    if (!resultado.IsSuccess || resultado.Value is null)
    {
        return Results.BadRequest(resultado.Message ?? "No se pudo generar la constancia.");
    }

    return Results.File(resultado.Value, "application/pdf");
}).RequireAuthorization();

// Equivalencia bachiller (hito 9.3c): impresión del listado de materias por
// equivalencia, servida inline (sucesor de lst_impresion_equivalencia_bac.pas, §3.3).
// El servidor revalida que la carrera sea de bachillerato (BAC/BAD).
app.MapGet("/constancias/alumno/equivalencia-bachiller", async (
    string carre,
    string cod,
    bool membrete,
    GenerarEquivalenciaBachillerHandler handler,
    CancellationToken ct) =>
{
    var resultado = await handler.GenerarPdfAsync(cod, carre, membrete, ct);
    if (!resultado.IsSuccess || resultado.Value is null)
    {
        return Results.BadRequest(resultado.Message ?? "No se pudo generar la equivalencia.");
    }

    return Results.File(resultado.Value, "application/pdf");
}).RequireAuthorization();

// Resolución de equivalencia terciaria (hito 9.3d): VISTO/CONSIDERANDO/RESUELVE para
// los cuatrimestres indicados, servida inline (sucesor del formato nuevo de
// lst_impresion_equivalencia_terc.pas). El servidor revalida que la carrera sea terciaria.
app.MapGet("/constancias/alumno/equivalencia-terciaria", async (
    string carre,
    string cod,
    string? cuatrimestres,
    bool membrete,
    GenerarResolucionEquivalenciaTerciariaHandler handler,
    CancellationToken ct) =>
{
    var resultado = await handler.GenerarPdfAsync(cod, carre, cuatrimestres, membrete, ct);
    if (!resultado.IsSuccess || resultado.Value is null)
    {
        return Results.BadRequest(resultado.Message ?? "No se pudo generar la resolución.");
    }

    return Results.File(resultado.Value, "application/pdf");
}).RequireAuthorization();

app.Run();
