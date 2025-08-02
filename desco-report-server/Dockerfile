# Multi-stage Dockerfile for DESC Report Server
# Supports both Docker and Podman

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["desco-report-server.csproj", "./"]

# Restore dependencies
RUN dotnet restore "desco-report-server.csproj"

# Copy source code
COPY . .

# Build the application
RUN dotnet build "desco-report-server.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "desco-report-server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN addgroup --system --gid 1001 desco && \
    adduser --system --uid 1001 --ingroup desco desco

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Change ownership to non-root user
RUN chown -R desco:desco /app
USER desco

# Expose port
EXPOSE 7654

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:7654/health || exit 1

# Set ASP.NET Core environment
ENV ASPNETCORE_URLS=http://+:7654
ENV ASPNETCORE_ENVIRONMENT=Production

# Start the application
ENTRYPOINT ["dotnet", "desco-report-server.dll"]