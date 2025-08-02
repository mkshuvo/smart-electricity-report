# DESC Report Server - Development Start Script (Windows PowerShell)
# This script starts the development environment

param(
    [switch]$Build = $false,
    [switch]$Logs = $false
)

Write-Host "Starting DESC Report Server Development Environment..." -ForegroundColor Green

# Check if Docker is running
try {
    docker info > $null 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Docker is not running. Please start Docker Desktop and try again." -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "Docker is not installed or not in PATH. Please install Docker Desktop." -ForegroundColor Red
    exit 1
}

# Create .env if it doesn't exist
if (-not (Test-Path ".\.env")) {
    Write-Host "Creating .env file..." -ForegroundColor Yellow
    @"
POSTGRES_USER=desco_user
POSTGRES_PASSWORD=desco_password
POSTGRES_DB=desco_report_db
REDIS_PASSWORD=redis_password
JWT_SECRET_KEY=your-super-secret-jwt-key-change-this-in-production
"@ | Out-File -FilePath ".\.env" -Encoding UTF8
    Write-Host ".env file created. Please update with your configuration." -ForegroundColor Green
}

# Build if requested
if ($Build) {
    Write-Host "Building Docker images..." -ForegroundColor Blue
    docker-compose build
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed. Please check the error messages above." -ForegroundColor Red
        exit 1
    }
}

# Start services
Write-Host "Starting development services..." -ForegroundColor Blue
docker-compose up -d

# Wait for services to be ready
Write-Host "Waiting for services to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Check service health
$services = @("desco-server", "postgres", "redis")
$maxAttempts = 30
$attempt = 0

foreach ($service in $services) {
    $healthy = $false
    while ($attempt -lt $maxAttempts -and -not $healthy) {
        $status = docker-compose ps $service --format json | ConvertFrom-Json
        if ($status.State -eq "running") {
            $healthy = $true
            Write-Host "$service is running ✓" -ForegroundColor Green
        } else {
            $attempt++
            Write-Host "Waiting for $service... ($attempt/$maxAttempts)" -ForegroundColor Yellow
            Start-Sleep -Seconds 2
        }
    }
    
    if (-not $healthy) {
        Write-Host "$service failed to start. Check logs with: docker-compose logs $service" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Development environment started successfully!" -ForegroundColor Green
Write-Host "API available at: http://localhost:8080" -ForegroundColor Blue
Write-Host "PostgreSQL available at: localhost:5432" -ForegroundColor Blue
Write-Host "Redis available at: localhost:6379" -ForegroundColor Blue
Write-Host ""
Write-Host "To view logs: docker-compose logs -f" -ForegroundColor Cyan
Write-Host "To stop: docker-compose down" -ForegroundColor Cyan

if ($Logs) {
    Write-Host "Opening logs..." -ForegroundColor Blue
    docker-compose logs -f
}