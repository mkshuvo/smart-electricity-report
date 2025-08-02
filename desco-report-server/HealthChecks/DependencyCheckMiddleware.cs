using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DESC.Report.Server.HealthChecks
{
    /// <summary>
    /// Middleware that ensures all dependencies are healthy before allowing requests
    /// </summary>
    public class DependencyCheckMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDependencyCheckerService _dependencyChecker;
        private readonly ILogger<DependencyCheckMiddleware> _logger;
        private static bool _dependenciesChecked = false;
        private static readonly object _lock = new();

        public DependencyCheckMiddleware(
            RequestDelegate next,
            IDependencyCheckerService dependencyChecker,
            ILogger<DependencyCheckMiddleware> logger)
        {
            _next = next;
            _dependencyChecker = dependencyChecker;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only check dependencies on first request
            if (!_dependenciesChecked)
            {
                lock (_lock)
                {
                    if (!_dependenciesChecked)
                    {
                        CheckDependenciesAsync(context.RequestAborted).GetAwaiter().GetResult();
                        _dependenciesChecked = true;
                    }
                }
            }

            await _next(context);
        }

        private async Task CheckDependenciesAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting dependency check before application startup...");

            var maxRetries = 30; // 30 retries with 2 second delays = 1 minute max wait
            var retryCount = 0;

            while (retryCount < maxRetries)
            {
                var result = await _dependencyChecker.CheckAllDependenciesAsync(cancellationToken);

                if (result.AllHealthy)
                {
                    _logger.LogInformation("All dependencies are healthy. Application startup proceeding.");
                    return;
                }

                var unhealthyServices = result.GetUnhealthyServices();
                _logger.LogWarning(
                    "Dependency check failed. Unhealthy services: {UnhealthyServices}. Retry {RetryCount}/{MaxRetries} in 2 seconds...",
                    string.Join(", ", unhealthyServices),
                    retryCount + 1,
                    maxRetries);

                retryCount++;
                await Task.Delay(2000, cancellationToken);
            }

            _logger.LogError("Maximum dependency check retries exceeded. Starting application with degraded services.");
        }
    }

    /// <summary>
    /// Extension methods for dependency check middleware
    /// </summary>
    public static class DependencyCheckMiddlewareExtensions
    {
        /// <summary>
        /// Add dependency check services to the container
        /// </summary>
        public static IServiceCollection AddDependencyCheck(this IServiceCollection services)
        {
            services.AddSingleton<IDependencyCheckerService, DependencyCheckerService>();
            services.AddScoped<IHealthCheckService, PostgreSqlHealthCheckService>();
            services.AddScoped<IHealthCheckService, RedisHealthCheckService>();
            
            return services;
        }

        /// <summary>
        /// Register a custom health check service
        /// </summary>
        public static IServiceCollection AddHealthCheck<T>(this IServiceCollection services) 
            where T : class, IHealthCheckService
        {
            services.AddScoped<IHealthCheckService, T>();
            return services;
        }

        /// <summary>
        /// Use dependency check middleware in the pipeline
        /// </summary>
        public static IApplicationBuilder UseDependencyCheck(this IApplicationBuilder app)
        {
            return app.UseMiddleware<DependencyCheckMiddleware>();
        }
    }

    /// <summary>
    /// Alternative approach: Use IHostedService for dependency checking
    /// </summary>
    public class DependencyCheckHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DependencyCheckHostedService> _logger;

        public DependencyCheckHostedService(
            IServiceProvider serviceProvider,
            ILogger<DependencyCheckHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Dependency check hosted service starting...");

            using var scope = _serviceProvider.CreateScope();
            var dependencyChecker = scope.ServiceProvider.GetRequiredService<IDependencyCheckerService>();

            var maxRetries = 30;
            var retryCount = 0;

            while (retryCount < maxRetries && !stoppingToken.IsCancellationRequested)
            {
                var result = await dependencyChecker.CheckAllDependenciesAsync(stoppingToken);

                if (result.AllHealthy)
                {
                    _logger.LogInformation("All dependencies are healthy. Application can proceed.");
                    return;
                }

                var unhealthyServices = result.GetUnhealthyServices();
                _logger.LogWarning(
                    "Dependency check failed. Unhealthy services: {UnhealthyServices}. Retry {RetryCount}/{MaxRetries}...",
                    string.Join(", ", unhealthyServices),
                    retryCount + 1,
                    maxRetries);

                retryCount++;
                await Task.Delay(2000, stoppingToken);
            }

            _logger.LogError("Maximum dependency check retries exceeded.");
        }
    }
}