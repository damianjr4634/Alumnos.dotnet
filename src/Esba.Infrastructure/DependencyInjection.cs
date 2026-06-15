using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Application.DTOs.Alumnos;
using Esba.Application.DTOs.Asistencias;
using Esba.Application.DTOs.Examenes;
using Esba.Application.Features.Academica;
using Esba.Application.Features.Administracion;
using Esba.Application.Features.Alumnos;
using Esba.Application.Features.Asistencias;
using Esba.Application.Features.Examenes;
using Esba.Application.Validators;
using Esba.Infrastructure.Excel;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Persistence.Repositories;
using Esba.Infrastructure.Queries;
using Esba.Infrastructure.Reports;
using Esba.Infrastructure.Security;
using Esba.Infrastructure.StoredProcedures;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Esba.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra la infraestructura. En Blazor Server se usa IDbContextFactory
    /// para evitar contextos compartidos entre renders concurrentes del mismo
    /// circuito (migration_improvements.md §2.3.2).
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Esba")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'Esba' en la configuración.");

        services.AddDbContextFactory<EsbaDbContext>(options => options.UseFirebird(connectionString));

        services.AddSingleton(new FbConnectionFactory(connectionString));
        services.AddScoped<IAlumnosQuery, AlumnosQuery>();
        services.AddScoped<ICarrerasQuery, CarrerasQuery>();

        // DbContext scoped resuelto desde la factory (un contexto por scope/circuito).
        services.AddScoped(sp => sp.GetRequiredService<IDbContextFactory<EsbaDbContext>>().CreateDbContext());
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        // Seguridad (§2.7): hash nuevo + cifrado legacy solo para la transición.
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<ILegacyPasswordCipher, EncriptoCadena2Cipher>();

        // Casos de uso y validadores de Application.
        services.AddScoped<IValidator<IniciarSesionCommand>, IniciarSesionValidator>();
        services.AddScoped<IniciarSesionHandler>();

        services.AddScoped<IAlumnoRepository, AlumnoRepository>();
        services.AddScoped<IValidator<CrearAlumnoCommand>, CrearAlumnoValidator>();
        services.AddScoped<IValidator<ActualizarAlumnoCommand>, ActualizarAlumnoValidator>();
        services.AddScoped<CrearAlumnoHandler>();
        services.AddScoped<ActualizarAlumnoHandler>();

        // Wrappers de SP legacy (§1.3: única vía de invocación de los XXX_*).
        services.AddScoped<ICambioDniLibroMatrizProcedure, CambioDniLibroMatrizProcedure>();
        services.AddScoped<ICuatrimestreVigenteProcedure, CuatrimestreVigenteProcedure>();

        // Académica: inscripción de materias.
        services.AddScoped<ICursadaQuery, CursadaQuery>();
        services.AddScoped<IMateriasQuery, MateriasQuery>();
        services.AddScoped<ICursadaRepository, CursadaRepository>();
        services.AddScoped<IMateriaRepository, MateriaRepository>();
        services.AddScoped<IValidator<InscribirEnMateriaCommand>, InscribirEnMateriaValidator>();
        services.AddScoped<IValidator<ModificarInscripcionCommand>, ModificarInscripcionValidator>();
        services.AddScoped<InscribirEnMateriaHandler>();
        services.AddScoped<ModificarInscripcionHandler>();
        services.AddScoped<EliminarInscripcionHandler>();

        // Académica: ABM de materias (hito 6).
        services.AddScoped<IValidator<CrearMateriaCommand>, CrearMateriaValidator>();
        services.AddScoped<IValidator<ActualizarMateriaCommand>, ActualizarMateriaValidator>();
        services.AddScoped<CrearMateriaHandler>();
        services.AddScoped<ActualizarMateriaHandler>();

        // Académica: comisiones y docentes (hito 6).
        services.AddScoped<IComisionesQuery, ComisionesQuery>();
        services.AddScoped<IDocentesQuery, DocentesQuery>();
        services.AddScoped<IComisionRepository, ComisionRepository>();
        services.AddScoped<IValidoComisionProcedure, ValidoComisionProcedure>();
        services.AddScoped<IValidator<CrearComisionCommand>, CrearComisionValidator>();
        services.AddScoped<IValidator<ActualizarComisionCommand>, ActualizarComisionValidator>();
        services.AddScoped<CrearComisionHandler>();
        services.AddScoped<ActualizarComisionHandler>();
        services.AddScoped<EliminarComisionHandler>();

        // Académica: inscripción masiva por cuatrimestre (deuda hito 6, dos fases).
        services.AddScoped<IInscripcionMasivaCuatrimestreProcedure, InscripcionMasivaCuatrimestreProcedure>();
        services.AddScoped<IValidator<InscribirCuatrimestreCompletoCommand>, InscribirCuatrimestreCompletoValidator>();
        services.AddScoped<InscribirCuatrimestreCompletoHandler>();

        // Asistencias: lectura (hito 7, increment 1).
        services.AddScoped<ITipoFaltasQuery, TipoFaltasQuery>();
        services.AddScoped<IFaltasComisionProcedure, FaltasComisionProcedure>();
        services.AddScoped<IFaltasAlumnoProcedure, FaltasAlumnoProcedure>();

        // Asistencias: carga de inasistencias por comisión (hito 7, increment 2).
        services.AddScoped<IInasistenciasRepository, InasistenciasRepository>();
        services.AddScoped<IValidator<GuardarInasistenciasComisionCommand>, GuardarInasistenciasComisionValidator>();
        services.AddScoped<GuardarInasistenciasComisionHandler>();

        // Asistencias: planillas + pase a libre (hito 7, increment 3).
        services.AddScoped<IPlanillaInasistenciasProcedure, PlanillaInasistenciasProcedure>();
        services.AddScoped<IPaseLibreProcedure, PaseLibreProcedure>();
        services.AddScoped<PasarMateriasALibreHandler>();

        // Exámenes: mesas (hito 8).
        services.AddScoped<IMesasQuery, MesasQuery>();
        services.AddScoped<ITipoMesaQuery, TipoMesaQuery>();
        services.AddScoped<IMesaRepository, MesaRepository>();
        services.AddScoped<IValidoMesaProcedure, ValidoMesaProcedure>();
        services.AddScoped<IValidator<CrearMesaCommand>, CrearMesaValidator>();
        services.AddScoped<IValidator<ActualizarMesaCommand>, ActualizarMesaValidator>();
        services.AddScoped<CrearMesaHandler>();
        services.AddScoped<ActualizarMesaHandler>();
        services.AddScoped<EliminarMesaHandler>();

        // Exámenes: permisos de examen (hito 8, increment 3).
        services.AddScoped<IMateriasFinalesProcedure, MateriasFinalesProcedure>();
        services.AddScoped<IPermisosExamenRepository, PermisosExamenRepository>();
        services.AddScoped<IValidator<CrearPermisoExamenCommand>, CrearPermisoExamenValidator>();
        services.AddScoped<CrearPermisoExamenHandler>();
        services.AddScoped<EliminarPermisoExamenHandler>();

        // Exportación de listados (hito 5): genéricos para EsbaListView. Sin estado.
        services.AddSingleton<IExcelExportService, ClosedXmlExportService>();
        services.AddSingleton<IPdfExportService, QuestPdfExportService>();

        return services;
    }
}
