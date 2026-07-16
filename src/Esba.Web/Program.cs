using Esba.Application.DTOs.Asistencias;
using Esba.Application.DTOs.Certificados;
using Esba.Application.DTOs.Examenes;
using Esba.Application.Features.Administracion;
using Esba.Application.Features.Asistencias;
using Esba.Application.Features.Certificados;
using Esba.Application.Features.Examenes;
using Esba.Domain.Asistencias;
using Esba.Domain.Enums;
using Esba.Domain.Examenes;
using Esba.Infrastructure;
using Esba.Web.Components;
using Esba.Web.Seguridad;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// DataProtection (hito 12.1): las claves que cifran la cookie de auth y el estado de
// los circuitos deben sobrevivir al redeploy del contenedor — si no, cada deploy
// desloguea a todos. En el stack de Portainer, DataProtection__KeysPath apunta a un
// volumen persistente; en desarrollo (sin la variable) alcanza el default del perfil
// del usuario.
var dataProtection = builder.Services.AddDataProtection().SetApplicationName("Esba");
var keysPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(keysPath))
{
    dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keysPath));
}
else if (builder.Environment.IsProduction())
{
    throw new InvalidOperationException(
        "Falta DataProtection__KeysPath: en producción las claves de DataProtection deben "
        + "persistirse en un volumen (hito 12.1); sin esto cada redeploy invalida las sesiones.");
}

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddInfrastructure(builder.Configuration);

// Estado del buscador de alumnos: persiste la búsqueda mientras dura el circuito.
builder.Services.AddScoped<Esba.Web.State.EstadoBusquedaAlumnos>();

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
    };

    var resultado = await handler.GenerarPdfAsync(command, conf, ct);
    if (!resultado.IsSuccess || resultado.Value is null)
    {
        return Results.BadRequest(resultado.Message ?? "No se pudo generar la constancia.");
    }

    return Results.File(resultado.Value, "application/pdf");
}).RequireAuthorization();

// Constancia de Alumno Regular (hito 10.4a): hoja A4 con membrete de fondo. El servidor
// revalida que el alumno esté CURSANDO/RECURSANDO en el cuatrimestre vigente (sucesor de
// constanciaalumnoregular.pas). Servida inline para previsualizar/imprimir (§3.3).
app.MapGet("/constancias/alumno/regular", async (
    string carre,
    string cod,
    string? ante,
    GenerarConstanciaRegularHandler handler,
    CancellationToken ct) =>
{
    var command = new GenerarConstanciaRegularCommand
    {
        CodigoCarrera = carre,
        CodigoAlumno = cod,
        AnteQuien = ante,
    };

    var resultado = await handler.GenerarPdfAsync(command, ct);
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
    GenerarConstanciaMateriasAprobadasHandler handler,
    CancellationToken ct) =>
{
    var resultado = await handler.GenerarPdfAsync(cod, carre, ante, ct);
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
    GenerarConstanciaExamenFinalHandler handler,
    CancellationToken ct) =>
{
    var resultado = await handler.GenerarPdfAsync(cod, carre, codmat, ante, ct);
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
    GenerarEquivalenciaBachillerHandler handler,
    CancellationToken ct) =>
{
    var resultado = await handler.GenerarPdfAsync(cod, carre, ct);
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
    GenerarResolucionEquivalenciaTerciariaHandler handler,
    CancellationToken ct) =>
{
    var resultado = await handler.GenerarPdfAsync(cod, carre, cuatrimestres, ct);
    if (!resultado.IsSuccess || resultado.Value is null)
    {
        return Results.BadRequest(resultado.Message ?? "No se pudo generar la resolución.");
    }

    return Results.File(resultado.Value, "application/pdf");
}).RequireAuthorization();

// Actas de examen por comisión (hito 14): A/REGULAR, Reincorporación o Exámenes.
// PDF Oficio inline (sucesor de lstactasARegular/lstactasreincorporacion/lstactasexamenes).
const string ExcelMime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

static GenerarActaComisionCommand ArmarComandoComision(
    TipoActaComision tipo, string carre, string cua, short? cutuco, string? codmat) => new()
{
    Tipo = tipo,
    CodigoCarrera = carre,
    CuatrimestreAnio = cua,
    Cutuco = cutuco,
    CodigoMateria = string.IsNullOrWhiteSpace(codmat) ? null : codmat,
};

app.MapGet("/actas/comision", async (
    string tipo, string carre, string cua, short? cutuco, string? codmat,
    GenerarActaComisionHandler handler, CancellationToken ct) =>
{
    if (!Enum.TryParse<TipoActaComision>(tipo, ignoreCase: true, out var tipoActa))
    {
        return Results.BadRequest("Tipo de acta inválido.");
    }

    var resultado = await handler.GenerarPdfAsync(ArmarComandoComision(tipoActa, carre, cua, cutuco, codmat), ct);
    return resultado.IsSuccess && resultado.Value is not null
        ? Results.File(resultado.Value, "application/pdf")
        : Results.BadRequest(resultado.Message ?? "No se pudo generar el acta.");
}).RequireAuthorization();

app.MapGet("/actas/comision/excel", async (
    string tipo, string carre, string cua, short? cutuco, string? codmat,
    GenerarActaComisionHandler handler, CancellationToken ct) =>
{
    if (!Enum.TryParse<TipoActaComision>(tipo, ignoreCase: true, out var tipoActa))
    {
        return Results.BadRequest("Tipo de acta inválido.");
    }

    var resultado = await handler.GenerarExcelAsync(ArmarComandoComision(tipoActa, carre, cua, cutuco, codmat), ct);
    return resultado.IsSuccess && resultado.Value is not null
        ? Results.File(resultado.Value, ExcelMime, $"acta_{tipo.ToLowerInvariant()}.xlsx")
        : Results.BadRequest(resultado.Message ?? "No se pudo generar el acta.");
}).RequireAuthorization();

// Acta volante por mesa (hito 14): PDF Oficio inline + Excel (sucesor de lstactasMesas).
app.MapGet("/actas/mesa", async (
    string carre, int mesa, string tipoExamen,
    GenerarActaMesaHandler handler, CancellationToken ct) =>
{
    var command = new GenerarActaMesaCommand { CodigoCarrera = carre, Mesa = mesa, TipoExamen = tipoExamen };
    var resultado = await handler.GenerarPdfAsync(command, ct);
    return resultado.IsSuccess && resultado.Value is not null
        ? Results.File(resultado.Value, "application/pdf")
        : Results.BadRequest(resultado.Message ?? "No se pudo generar el acta.");
}).RequireAuthorization();

app.MapGet("/actas/mesa/excel", async (
    string carre, int mesa, string tipoExamen,
    GenerarActaMesaHandler handler, CancellationToken ct) =>
{
    var command = new GenerarActaMesaCommand { CodigoCarrera = carre, Mesa = mesa, TipoExamen = tipoExamen };
    var resultado = await handler.GenerarExcelAsync(command, ct);
    return resultado.IsSuccess && resultado.Value is not null
        ? Results.File(resultado.Value, ExcelMime, $"acta_mesa_{mesa}.xlsx")
        : Results.BadRequest(resultado.Message ?? "No se pudo generar el acta.");
}).RequireAuthorization();

// Carpetas por comisión (planillas en blanco de asistencia, trabajos prácticos o
// calificaciones para el docente): PDF inline y export Excel (sucesor de
// lstplanasis.pas y lstNotasyPractico.pas).
app.MapGet("/asistencias/carpeta/pdf", async (
    string tipo, string carre, string cua, short? cutuco, string? codmat,
    GenerarCarpetaComisionHandler handler, CancellationToken ct) =>
{
    if (!Enum.TryParse<TipoCarpetaComision>(tipo, ignoreCase: true, out var tipoCarpeta))
    {
        return Results.BadRequest("Tipo de carpeta inválido.");
    }

    var command = new GenerarCarpetaComisionCommand
    {
        Tipo = tipoCarpeta,
        CodigoCarrera = carre,
        CuatrimestreAnio = cua,
        Cutuco = cutuco,
        CodigoMateria = string.IsNullOrWhiteSpace(codmat) ? null : codmat,
    };
    var resultado = await handler.GenerarPdfAsync(command, ct);
    return resultado.IsSuccess && resultado.Value is not null
        ? Results.File(resultado.Value, "application/pdf")
        : Results.BadRequest(resultado.Message ?? "No se pudo generar la carpeta.");
}).RequireAuthorization();

app.MapGet("/asistencias/carpeta/excel", async (
    string tipo, string carre, string cua, short? cutuco, string? codmat,
    GenerarCarpetaComisionHandler handler, CancellationToken ct) =>
{
    if (!Enum.TryParse<TipoCarpetaComision>(tipo, ignoreCase: true, out var tipoCarpeta))
    {
        return Results.BadRequest("Tipo de carpeta inválido.");
    }

    var command = new GenerarCarpetaComisionCommand
    {
        Tipo = tipoCarpeta,
        CodigoCarrera = carre,
        CuatrimestreAnio = cua,
        Cutuco = cutuco,
        CodigoMateria = string.IsNullOrWhiteSpace(codmat) ? null : codmat,
    };
    var resultado = await handler.GenerarExcelAsync(command, ct);
    var nombre = tipoCarpeta == TipoCarpetaComision.TrabajosPracticos
        ? "trabajos_practicos"
        : "planilla_calificaciones";
    return resultado.IsSuccess && resultado.Value is not null
        ? Results.File(resultado.Value, ExcelMime, $"{nombre}_{carre}_{cua.Replace('/', '-')}.xlsx")
        : Results.BadRequest(resultado.Message ?? "No se pudo generar la carpeta.");
}).RequireAuthorization();

app.Run();
