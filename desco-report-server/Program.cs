using DESC.Report.Server.HealthChecks;
using desco_report_server.Data;
using desco_report_server.Models;
using desco_report_server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using Serilog;
using System.Text;

namespace desco_report_server;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();
        
        // Use Serilog for logging
        builder.Host.UseSerilog();

        // Add configuration
        builder.Configuration.AddEnvironmentVariables();
        
        // Add dependency check configuration
        builder.Services.Configure<DependencyCheckOptions>(
            builder.Configuration.GetSection("DependencyCheck"));

        // Add database context
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Add Identity
        builder.Services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Add Redis
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configuration = builder.Configuration.GetConnectionString("Redis");
            if (string.IsNullOrEmpty(configuration))
            {
                throw new InvalidOperationException("Redis connection string is not configured. Please set the 'Redis' connection string in your configuration.");
            }
            return ConnectionMultiplexer.Connect(configuration);
        });

        // Add dependency checking services
        builder.Services.AddDependencyCheck();

        // Add hosted service for dependency checking
        builder.Services.AddHostedService<DependencyCheckHostedService>();

        // Add services to the container.
        builder.Services.AddAuthorization();
        
        // Add database seeder
        builder.Services.AddTransient<DatabaseSeeder>();

        // Add JWT service
        builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IDescoApiService, DescoApiService>();

    // Add HTTP client for DESCO API
    builder.Services.AddHttpClient("DescoApi", client =>
    {
        client.BaseAddress = new Uri("https://prepaid.desco.org.bd");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("User-Agent", "DESCO-Report-Server/1.0");
    });

    // Add authentication
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "Bearer";
            options.DefaultChallengeScheme = "Bearer";
        })
        .AddJwtBearer("Bearer", options =>
        {
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
            };
        });

        // Configure CORS for frontend integration
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Use CORS middleware
        app.UseCors("AllowFrontend");

        // Use dependency check middleware
        app.UseDependencyCheck();

        app.UseAuthentication();
        app.UseAuthorization();

        // Seed database on startup
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var seeder = services.GetRequiredService<DatabaseSeeder>();
                await seeder.SeedAsync();
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        app.Run();
    }
}