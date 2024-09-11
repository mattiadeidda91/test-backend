using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Test.Backend.Abstractions.HealthChecks
{
    public class DbContextHealthCheck<TContext> : IHealthCheck where TContext : DbContext
    {
        private readonly TContext _dbContext;

        public DbContextHealthCheck(TContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // check database connection
                await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
                return HealthCheckResult.Healthy("Database is reachable.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Database is not reachable.", ex);
            }
        }
    }
}
