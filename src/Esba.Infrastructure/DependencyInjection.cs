using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Application.DTOs.Alumnos;
using Esba.Application.Features.Academica;
using Esba.Application.Features.Administracion;
using Esba.Application.Features.Alumnos;
using Esba.Application.Validators;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Persistence.Repositories;
using Esba.Infrastructure.Queries;
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

        return services;
    }
}
