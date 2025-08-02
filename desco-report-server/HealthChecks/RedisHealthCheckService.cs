using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace DESC.Report.Server.HealthChecks
{
    /// <summary>
    /// Health check service for Redis cache
    /// </summary>
    public class RedisHealthCheckService : IHealthCheckService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisHealthCheckService> _logger;

        public string ServiceName => "Redis";

        public RedisHealthCheckService(
            IConnectionMultiplexer redis,
            ILogger<RedisHealthCheckService> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                var database = _redis.GetDatabase();
                
                // Test Redis connectivity with PING command
                var pingResult = await database.PingAsync();
                
                stopwatch.Stop();
                
                _logger.LogDebug("Redis health check passed in {ResponseTime}ms (ping: {Ping}ms)", 
                    stopwatch.ElapsedMilliseconds, pingResult.TotalMilliseconds);
                
                return new HealthCheckResult
                {
                    IsHealthy = true,
                    ServiceName = ServiceName,
                    ResponseTime = stopwatch.Elapsed,
                    Metadata = new Dictionary<string, object>
                    {
                        ["PingResponseTime"] = pingResult.TotalMilliseconds,
                        ["ServerVersion"] = _redis.GetServer(_redis.GetEndPoints().First()).Version.ToString()
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Redis health check failed after {ResponseTime}ms", stopwatch.ElapsedMilliseconds);
                
                return HealthCheckResult.Unhealthy(
                    ServiceName, 
                    $"Redis connection failed: {ex.Message}", 
                    stopwatch.Elapsed);
            }
        }
    }
}