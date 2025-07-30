using ConwayGameOfLife.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ConwayGameOfLife.Core;

/// <summary>
/// Extension methods for setting up core services in an <see cref="IServiceCollection" />.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds core services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // Add services
        services.AddScoped<IGameOfLifeService, GameOfLifeService>();

        return services;
    }
}