namespace DESC.Report.Server.HealthChecks
{
    /// <summary>
    /// Interface for health check services
    /// </summary>
    public interface IHealthCheckService
    {
        /// <summary>
        /// Service name for identification
        /// </summary>
        string ServiceName { get; }

        /// <summary>
        /// Check if the service is healthy
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Health check result</returns>
        Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Result of a health check
    /// </summary>
    public class HealthCheckResult
    {
        public bool IsHealthy { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object>? Metadata { get; set; }

        public static HealthCheckResult Healthy(string serviceName, TimeSpan responseTime)
        {
            return new HealthCheckResult
            {
                IsHealthy = true,
                ServiceName = serviceName,
                ResponseTime = responseTime
            };
        }

        public static HealthCheckResult Unhealthy(string serviceName, string errorMessage, TimeSpan responseTime)
        {
            return new HealthCheckResult
            {
                IsHealthy = false,
                ServiceName = serviceName,
                ErrorMessage = errorMessage,
                ResponseTime = responseTime
            };
        }
    }
}