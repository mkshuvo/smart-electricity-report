namespace DESC.Report.Server.HealthChecks
{
    /// <summary>
    /// Configuration options for dependency checking
    /// </summary>
    public class DependencyCheckOptions
    {
        /// <summary>
        /// Enable or disable dependency checking
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Maximum number of retries for dependency checks
        /// </summary>
        public int MaxRetries { get; set; } = 30;

        /// <summary>
        /// Delay between retries in milliseconds
        /// </summary>
        public int RetryDelayMilliseconds { get; set; } = 2000;

        /// <summary>
        /// Services to exclude from dependency checking
        /// </summary>
        public List<string> ExcludedServices { get; set; } = new();

        /// <summary>
        /// Timeout for individual health checks in milliseconds
        /// </summary>
        public int HealthCheckTimeoutMilliseconds { get; set; } = 5000;
    }

    /// <summary>
    /// Template for adding new health check services
    /// </summary>
    public abstract class HealthCheckTemplate : IHealthCheckService
    {
        public abstract string ServiceName { get; }

        public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                var isHealthy = await PerformHealthCheckAsync(cancellationToken);
                stopwatch.Stop();

                return isHealthy 
                    ? HealthCheckResult.Healthy(ServiceName, stopwatch.Elapsed)
                    : HealthCheckResult.Unhealthy(ServiceName, "Health check returned false", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return HealthCheckResult.Unhealthy(ServiceName, ex.Message, stopwatch.Elapsed);
            }
        }

        /// <summary>
        /// Implement this method to perform the actual health check
        /// </summary>
        protected abstract Task<bool> PerformHealthCheckAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Helper method to create metadata for health check results
        /// </summary>
        protected Dictionary<string, object> CreateMetadata(params (string key, object value)[] items)
        {
            var metadata = new Dictionary<string, object>();
            foreach (var (key, value) in items)
            {
                metadata[key] = value;
            }
            return metadata;
        }
    }

    /// <summary>
    /// Example implementation for adding new services
    /// </summary>
    public class ExampleHealthCheckService : HealthCheckTemplate
    {
        public override string ServiceName => "ExampleService";

        protected override async Task<bool> PerformHealthCheckAsync(CancellationToken cancellationToken)
        {
            // Implement your health check logic here
            // Example: Check if an external API is reachable
            
            // using var httpClient = new HttpClient();
            // var response = await httpClient.GetAsync("https://api.example.com/health", cancellationToken);
            // return response.IsSuccessStatusCode;
            
            // For demonstration, return true
            await Task.Delay(100, cancellationToken);
            return true;
        }
    }
}