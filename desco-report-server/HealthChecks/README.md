# Dependency Checker System

A dynamic, extensible dependency checking system that ensures all required services are healthy before allowing the application to start.

## Features

- **Dynamic Service Registration**: Add new health checks without modifying core code
- **Retry Logic**: Automatic retries with configurable delays
- **Comprehensive Logging**: Detailed logs for debugging dependency issues
- **Flexible Configuration**: Configurable via appsettings.json
- **Template Support**: Easy-to-use templates for new services
- **Multiple Integration Options**: Middleware, hosted service, or manual checking

## Quick Start

### 1. Basic Setup

The system is already configured in Program.cs. Services are automatically registered and checked on startup.

### 2. Configuration

Update `appsettings.json`:

```json
{
  "DependencyCheck": {
    "Enabled": true,
    "MaxRetries": 30,
    "RetryDelayMilliseconds": 2000,
    "HealthCheckTimeoutMilliseconds": 5000,
    "ExcludedServices": []
  }
}
```

### 3. Adding New Health Checks

#### Method 1: Using the Template (Recommended)

Create a new health check by inheriting from `HealthCheckTemplate`:

```csharp
public class MyServiceHealthCheck : HealthCheckTemplate
{
    public override string ServiceName => "MyService";
    
    private readonly IMyServiceClient _client;
    
    public MyServiceHealthCheck(IMyServiceClient client)
    {
        _client = client;
    }
    
    protected override async Task<bool> PerformHealthCheckAsync(CancellationToken cancellationToken)
    {
        // Implement your health check logic
        var result = await _client.CheckHealthAsync(cancellationToken);
        return result.IsHealthy;
    }
}
```

Register in Program.cs:

```csharp
builder.Services.AddHealthCheck<MyServiceHealthCheck>();
```

#### Method 2: Implement IHealthCheckService Directly

```csharp
public class CustomHealthCheck : IHealthCheckService
{
    public string ServiceName => "CustomService";
    
    public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Your health check logic
            return HealthCheckResult.Healthy(ServiceName, TimeSpan.Zero);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ServiceName, ex.Message, TimeSpan.Zero);
        }
    }
}
```

### 4. Service Registration Patterns

#### Register Single Service
```csharp
services.AddHealthCheck<MyCustomHealthCheck>();
```

#### Register Multiple Services
```csharp
services.AddHealthCheck<ServiceAHealthCheck>();
services.AddHealthCheck<ServiceBHealthCheck>();
services.AddHealthCheck<ServiceCHealthCheck>();
```

#### Register with Custom Interface
```csharp
services.AddScoped<IHealthCheckService, MyServiceHealthCheck>();
```

## Available Health Checks

### PostgreSQL Health Check
- **Service Name**: "PostgreSQL"
- **Checks**: Connection availability, basic query execution
- **Configuration**: Uses configured connection string

### Redis Health Check
- **Service Name**: "Redis"
- **Checks**: Connection, PING response, server version
- **Configuration**: Uses configured Redis connection string

## Monitoring and Debugging

### Log Levels
- **Debug**: Individual health check results
- **Information**: Overall dependency check status
- **Warning**: Failed dependency checks (with retries)
- **Error**: Maximum retries exceeded

### Health Check Endpoints

You can add a health check endpoint:

```csharp
app.MapGet("/health", async (IDependencyCheckerService checker) =>
{
    var result = await checker.CheckAllDependenciesAsync();
    return Results.Ok(new 
    {
        status = result.AllHealthy ? "healthy" : "unhealthy",
        timestamp = result.Timestamp,
        totalTime = result.TotalCheckTime.TotalMilliseconds,
        services = result.Results.Select(r => new 
        {
            service = r.ServiceName,
            healthy = r.IsHealthy,
            responseTime = r.ResponseTime.TotalMilliseconds,
            error = r.ErrorMessage
        })
    });
});
```

## Advanced Usage

### Custom Configuration

```csharp
builder.Services.Configure<DependencyCheckOptions>(options =>
{
    options.MaxRetries = 50;
    options.RetryDelayMilliseconds = 1000;
    options.ExcludedServices.Add("OptionalService");
});
```

### Manual Dependency Checking

```csharp
public class MyService
{
    private readonly IDependencyCheckerService _checker;
    
    public MyService(IDependencyCheckerService checker)
    {
        _checker = checker;
    }
    
    public async Task<bool> CheckDependencies()
    {
        var result = await _checker.CheckAllDependenciesAsync();
        return result.AllHealthy;
    }
}
```

### Conditional Health Checks

```csharp
public class ConditionalHealthCheck : HealthCheckTemplate
{
    public override string ServiceName => "ConditionalService";
    
    private readonly IConfiguration _config;
    
    public ConditionalHealthCheck(IConfiguration config)
    {
        _config = config;
    }
    
    protected override async Task<bool> PerformHealthCheckAsync(CancellationToken cancellationToken)
    {
        var isEnabled = _config.GetValue<bool>("Features:ConditionalService");
        if (!isEnabled) return true; // Skip if disabled
        
        // Perform actual check
        return await CheckServiceHealthAsync(cancellationToken);
    }
}
```

## Testing

### Unit Test Example

```csharp
[TestClass]
public class HealthCheckTests
{
    [TestMethod]
    public async Task PostgreSqlHealthCheck_ReturnsHealthy_WhenDatabaseIsAvailable()
    {
        // Arrange
        var mockDb = new Mock<ApplicationDbContext>();
        mockDb.Setup(x => x.Database.CanConnectAsync(default)).ReturnsAsync(true);
        
        var service = new PostgreSqlHealthCheckService(
            mockDb.Object, 
            Mock.Of<ILogger<PostgreSqlHealthCheckService>>(),
            Mock.Of<IConfiguration>());
            
        // Act
        var result = await service.CheckHealthAsync();
        
        // Assert
        Assert.IsTrue(result.IsHealthy);
    }
}
```

## Troubleshooting

### Common Issues

1. **Service Not Registered**: Ensure health check is added to DI container
2. **Connection String Missing**: Check appsettings.json configuration
3. **Timeout Issues**: Increase `HealthCheckTimeoutMilliseconds`
4. **Service Dependencies**: Ensure required services are registered before health checks

### Debug Logging

Enable debug logging for health checks:

```json
{
  "Logging": {
    "LogLevel": {
      "DESC.Report.Server.HealthChecks": "Debug"
    }
  }
}
```