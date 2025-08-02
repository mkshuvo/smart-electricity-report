using desco_report_server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DESC.Report.Server.HealthChecks
{
    /// <summary>
    /// Health check service for PostgreSQL database
    /// </summary>
    public class PostgreSqlHealthCheckService : IHealthCheckService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<PostgreSqlHealthCheckService> _logger;
        private readonly IConfiguration _configuration;

        public string ServiceName => "PostgreSQL";

        public PostgreSqlHealthCheckService(
            ApplicationDbContext dbContext,
            ILogger<PostgreSqlHealthCheckService> logger,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                // Test database connection
                await _dbContext.Database.CanConnectAsync(cancellationToken);
                
                // Test basic query execution
                await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
                
                stopwatch.Stop();
                
                _logger.LogDebug("PostgreSQL health check passed in {ResponseTime}ms", stopwatch.ElapsedMilliseconds);
                
                return HealthCheckResult.Healthy(ServiceName, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "PostgreSQL health check failed after {ResponseTime}ms", stopwatch.ElapsedMilliseconds);
                
                return HealthCheckResult.Unhealthy(
                    ServiceName, 
                    $"PostgreSQL connection failed: {ex.Message}", 
                    stopwatch.Elapsed);
            }
        }
    }
}