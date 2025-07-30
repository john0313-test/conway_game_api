using ConwayGameOfLife.Core.Repositories;
using ConwayGameOfLife.Infrastructure.Data;
using ConwayGameOfLife.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConwayGameOfLife.Infrastructure;

/// <summary>
/// Extension methods for setting up infrastructure services in an <see cref="IServiceCollection" />.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add database context
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        // Add repositories
        services.AddScoped<IBoardStateRepository, BoardStateRepository>();

        return services;
    }
}