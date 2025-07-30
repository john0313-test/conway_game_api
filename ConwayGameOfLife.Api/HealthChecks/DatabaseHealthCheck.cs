using ConwayGameOfLife.Infrastructure.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ConwayGameOfLife.Api.HealthChecks;

/// <summary>
/// Health check for the database
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly AppDbContext _dbContext;

    /// <summary>
    /// Creates a new instance of the database health check
    /// </summary>
    /// <param name="dbContext">The database context</param>
    public DatabaseHealthCheck(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Checks the health of the database
    /// </summary>
    /// <param name="context">The health check context</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The health check result</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if the database is available
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            
            if (canConnect)
            {
                return HealthCheckResult.Healthy("Database is healthy");
            }
            
            return HealthCheckResult.Unhealthy("Database is unhealthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database health check failed", ex);
        }
    }
}