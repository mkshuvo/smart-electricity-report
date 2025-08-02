# DESC Report Server - Production Start Script (Windows PowerShell)
# This script starts the production environment

param(
    [switch]$Build = $false,
    [switch]$Logs = $false
)

Write-Host "Starting DESC Report Server Production Environment..." -ForegroundColor Green

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

# Check if .env exists and contains production values
if (-not (Test-Path ".\.env")) {
    Write-Host ".env file not found. Creating from template..." -ForegroundColor Yellow
    Copy-Item ".\.env.example" ".\.env"
    Write-Host "Please update .env with production values before continuing." -ForegroundColor Red
    exit 1
}

# Validate critical environment variables
$envContent = Get-Content ".\.env" -Raw
$requiredVars = @("POSTGRES_PASSWORD", "REDIS_PASSWORD", "JWT_SECRET_KEY")

foreach ($var in $requiredVars) {
    if ($envContent -match "$var=change_this_" -or $envContent -match "$var=your-super-secret") {
        Write-Host "WARNING: $var is using default value. Please update for production." -ForegroundColor Red
        $response = Read-Host "Continue anyway? (y/N)"
        if ($response -ne "y" -and $response -ne "Y") {
            exit 1
        }
    }
}

# Build if requested
if ($Build) {
    Write-Host "Building production Docker images..." -ForegroundColor Blue
    docker-compose -f docker-compose.prod.yml build --no-cache
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed. Please check the error messages above." -ForegroundColor Red
        exit 1
    }
}

# Start services
Write-Host "Starting production services..." -ForegroundColor Blue
docker-compose -f docker-compose.prod.yml up -d

# Wait for services to be ready
Write-Host "Waiting for production services to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

# Check service health
$services = @("desco-server", "postgres", "redis")
$maxAttempts = 30

foreach ($service in $services) {
    $healthy = $false
    $attempt = 0
    while ($attempt -lt $maxAttempts -and -not $healthy) {
        $status = docker-compose -f docker-compose.prod.yml ps $service --format json | ConvertFrom-Json
        if ($status.State -eq "running") {
            $healthy = $true
            Write-Host "$service is running ✓" -ForegroundColor Green
        } else {
            $attempt++
            Write-Host "Waiting for $service... ($attempt/$maxAttempts)" -ForegroundColor Yellow
            Start-Sleep -Seconds 3
        }
    }
    
    if (-not $healthy) {
        Write-Host "$service failed to start. Check logs with: docker-compose -f docker-compose.prod.yml logs $service" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Production environment started successfully!" -ForegroundColor Green
Write-Host "API available at: http://localhost:8080" -ForegroundColor Blue
Write-Host "PostgreSQL available at: localhost:5432" -ForegroundColor Blue
Write-Host "Redis available at: localhost:6379" -ForegroundColor Blue
Write-Host ""
Write-Host "IMPORTANT: Update your .env file with production values!" -ForegroundColor Red
Write-Host "To view logs: docker-compose -f docker-compose.prod.yml logs -f" -ForegroundColor Cyan
Write-Host "To stop: docker-compose -f docker-compose.prod.yml down" -ForegroundColor Cyan

if ($Logs) {
    Write-Host "Opening production logs..." -ForegroundColor Blue
    docker-compose -f docker-compose.prod.yml logs -f
}