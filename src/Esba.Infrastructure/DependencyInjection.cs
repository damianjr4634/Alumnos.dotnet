using Esba.Application.Abstractions;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Queries;
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

        return services;
    }
}
