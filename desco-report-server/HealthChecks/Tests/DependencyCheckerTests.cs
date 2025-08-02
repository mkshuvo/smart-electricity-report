using Microsoft.Extensions.DependencyInjection;

namespace DESC.Report.Server.HealthChecks.Tests;

/// <summary>
/// Simple test class for dependency checker functionality
/// These are basic tests that don't require external test frameworks
/// For proper unit testing, consider adding xUnit/NUnit and Moq packages
/// </summary>
public static class DependencyCheckerTests
{
    public static void RunBasicTests()
    {
        Console.WriteLine("Running basic dependency checker tests...");
        
        TestHealthCheckResultCreation();
        TestDependencyCheckOptions();
        TestServiceRegistration();
        
        Console.WriteLine("Basic tests completed successfully!");
    }
    
    private static void TestHealthCheckResultCreation()
    {
        var healthyResult = HealthCheckResult.Healthy("TestService", TimeSpan.FromMilliseconds(100));
        var unhealthyResult = HealthCheckResult.Unhealthy("TestService", "Connection failed", TimeSpan.FromMilliseconds(50));
        
        if (!healthyResult.IsHealthy) throw new Exception("Healthy result test failed");
        if (unhealthyResult.IsHealthy) throw new Exception("Unhealthy result test failed");
        
        Console.WriteLine("✓ HealthCheckResult creation tests passed");
    }
    
    private static void TestDependencyCheckOptions()
    {
        var options = new DependencyCheckOptions
        {
            Enabled = true,
            MaxRetries = 5,
            RetryDelayMilliseconds = 1000,
            HealthCheckTimeoutMilliseconds = 3000
        };
        
        if (!options.Enabled || options.MaxRetries != 5)
            throw new Exception("DependencyCheckOptions test failed");
            
        Console.WriteLine("✓ DependencyCheckOptions tests passed");
    }
    
    private static void TestServiceRegistration()
    {
        var services = new ServiceCollection();
        
        // Test basic service registration
        services.AddSingleton<IHealthCheckService, PostgreSqlHealthCheckService>();
        services.AddSingleton<IHealthCheckService, RedisHealthCheckService>();
        
        var provider = services.BuildServiceProvider();
        var healthChecks = provider.GetServices<IHealthCheckService>().ToList();
        
        if (healthChecks.Count != 2)
            throw new Exception("Service registration test failed");
            
        Console.WriteLine("✓ Service registration tests passed");
    }
}