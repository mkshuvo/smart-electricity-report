using Microsoft.Extensions.Logging;

namespace DESC.Report.Server.HealthChecks
{
    /// <summary>
    /// Central service for checking all dependencies before application startup
    /// </summary>
    public interface IDependencyCheckerService
    {
        /// <summary>
        /// Check all registered dependencies
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Overall dependency check result</returns>
        Task<DependencyCheckResult> CheckAllDependenciesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Register a new health check service
        /// </summary>
        /// <param name="healthCheckService">Health check service to register</param>
        void RegisterHealthCheck(IHealthCheckService healthCheckService);
    }

    /// <summary>
    /// Result of overall dependency check
    /// </summary>
    public class DependencyCheckResult
    {
        public bool AllHealthy { get; set; }
        public List<HealthCheckResult> Results { get; set; } = new();
        public TimeSpan TotalCheckTime { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public List<string> GetUnhealthyServices()
        {
            return Results.Where(r => !r.IsHealthy)
                         .Select(r => r.ServiceName)
                         .ToList();
        }
    }

    /// <summary>
    /// Implementation of dependency checker service
    /// </summary>
    public class DependencyCheckerService : IDependencyCheckerService
    {
        private readonly List<IHealthCheckService> _healthCheckServices = new();
        private readonly ILogger<DependencyCheckerService> _logger;

        public DependencyCheckerService(ILogger<DependencyCheckerService> logger)
        {
            _logger = logger;
        }

        public void RegisterHealthCheck(IHealthCheckService healthCheckService)
        {
            if (healthCheckService == null)
                throw new ArgumentNullException(nameof(healthCheckService));

            _healthCheckServices.Add(healthCheckService);
            _logger.LogInformation("Registered health check service: {ServiceName}", healthCheckService.ServiceName);
        }

        public async Task<DependencyCheckResult> CheckAllDependenciesAsync(CancellationToken cancellationToken = default)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var results = new List<HealthCheckResult>();

            _logger.LogInformation("Starting dependency check for {ServiceCount} services", _healthCheckServices.Count);

            var tasks = _healthCheckServices.Select(async service =>
            {
                try
                {
                    return await service.CheckHealthAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Health check failed for service {ServiceName}", service.ServiceName);
                    return HealthCheckResult.Unhealthy(service.ServiceName, ex.Message, TimeSpan.Zero);
                }
            });

            results.AddRange(await Task.WhenAll(tasks));
            stopwatch.Stop();

            var allHealthy = results.All(r => r.IsHealthy);

            _logger.LogInformation(
                "Dependency check completed in {TotalTime}ms. Healthy: {HealthyCount}/{TotalCount}",
                stopwatch.ElapsedMilliseconds,
                results.Count(r => r.IsHealthy),
                results.Count);

            return new DependencyCheckResult
            {
                AllHealthy = allHealthy,
                Results = results,
                TotalCheckTime = stopwatch.Elapsed
            };
        }
    }
}