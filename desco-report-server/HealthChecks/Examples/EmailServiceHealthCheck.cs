using DESC.Report.Server.HealthChecks;

namespace desco_report_server.HealthChecks.Examples;

/// <summary>
/// Example health check for an email service using SMTP
/// Demonstrates how to add new services to the dependency checker
/// </summary>
public class EmailServiceHealthCheck(
    IConfiguration configuration, 
    ILogger<EmailServiceHealthCheck> logger) : HealthCheckTemplate
{
    public override string ServiceName => "EmailService";
    
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<EmailServiceHealthCheck> _logger = logger;
    
    protected override async Task<bool> PerformHealthCheckAsync(CancellationToken cancellationToken)
    {
        try
        {
            var smtpHost = _configuration["Email:Smtp:Host"];
            if (string.IsNullOrEmpty(smtpHost))
            {
                _logger.LogWarning("Email SMTP configuration missing");
                return false;
            }
            
            // Simulate async SMTP server connectivity check
            await Task.Delay(50, cancellationToken);
            
            // Simulate async DNS resolution
            await Task.Run(() => 
            {
                try
                {
                    System.Net.Dns.GetHostAddresses(smtpHost);
                }
                catch (Exception)
                {
                    throw new Exception($"Unable to resolve SMTP host: {smtpHost}");
                }
            }, cancellationToken);
            
            _logger.LogDebug("Email service health check passed");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email service health check failed");
            return false;
        }
    }
}

/// <summary>
/// Example health check for an external API service
/// </summary>
public class ExternalApiHealthCheck(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<ExternalApiHealthCheck> logger) : HealthCheckTemplate
{
    public override string ServiceName => "ExternalApi";
    
    private readonly HttpClient _httpClient = httpClient;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<ExternalApiHealthCheck> _logger = logger;
    
    protected override async Task<bool> PerformHealthCheckAsync(CancellationToken cancellationToken)
    {
        try
        {
            var apiUrl = _configuration["ExternalApi:HealthEndpoint"];
            if (string.IsNullOrEmpty(apiUrl))
            {
                _logger.LogWarning("External API health endpoint not configured");
                return false;
            }
            
            // Perform actual HTTP health check
            var response = await _httpClient.GetAsync(apiUrl, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            _logger.LogDebug("External API health check passed");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "External API health check failed");
            return false;
        }
    }
}