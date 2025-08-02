# DESC Report Server - Utility Scripts (Windows PowerShell)
# Collection of utility functions for Docker management

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("logs", "health", "status", "clean", "clean-all", "backup-db", "restore-db", "shell", "db-shell", "redis-shell")]
    [string]$Command,
    
    [string]$BackupFile,
    [string]$RestoreFile
)

function Show-Help {
    Write-Host "DESC Report Server - Docker Utilities" -ForegroundColor Green
    Write-Host ""
    Write-Host "Usage: .\utils.ps1 -Command <command> [options]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Available Commands:" -ForegroundColor Cyan
    Write-Host "  logs        - View all container logs"
    Write-Host "  health      - Check service health"
    Write-Host "  status      - Show container status"
    Write-Host "  clean       - Stop and remove containers"
    Write-Host "  clean-all   - Clean everything including volumes"
    Write-Host "  backup-db   - Create database backup"
    Write-Host "  restore-db  - Restore database from backup"
    Write-Host "  shell       - Access application shell"
    Write-Host "  db-shell    - Access PostgreSQL shell"
    Write-Host "  redis-shell - Access Redis CLI"
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Cyan
    Write-Host "  .\utils.ps1 -Command logs"
    Write-Host "  .\utils.ps1 -Command backup-db -BackupFile backup.sql"
    Write-Host "  .\utils.ps1 -Command restore-db -RestoreFile backup.sql"
}

function Test-DockerRunning {
    try {
        docker info > $null 2>&1
        return $true
    } catch {
        Write-Host "Docker is not running. Please start Docker Desktop." -ForegroundColor Red
        return $false
    }
}

function Get-ActiveComposeFile {
    # Check which compose file is being used
    $devRunning = docker-compose ps --services 2>$null
    $prodRunning = docker-compose -f docker-compose.prod.yml ps --services 2>$null
    
    if ($devRunning) { return "docker-compose.yml" }
    elseif ($prodRunning) { return "docker-compose.prod.yml" }
    else { return "docker-compose.yml" }
}

switch ($Command) {
    "logs" {
        if (-not (Test-DockerRunning)) { exit 1 }
        $composeFile = Get-ActiveComposeFile
        Write-Host "Viewing logs from $composeFile..." -ForegroundColor Blue
        docker-compose -f $composeFile logs -f
    }
    
    "health" {
        if (-not (Test-DockerRunning)) { exit 1 }
        $composeFile = Get-ActiveComposeFile
        Write-Host "Service Health Check:" -ForegroundColor Green
        docker-compose -f $composeFile ps
        Write-Host ""
        Write-Host "Docker Images:" -ForegroundColor Green
        docker images | Select-String "desco"
    }
    
    "status" {
        if (-not (Test-DockerRunning)) { exit 1 }
        $composeFile = Get-ActiveComposeFile
        Write-Host "Container Status:" -ForegroundColor Green
        docker-compose -f $composeFile ps
    }
    
    "clean" {
        if (-not (Test-DockerRunning)) { exit 1 }
        $composeFile = Get-ActiveComposeFile
        Write-Host "Stopping and cleaning containers..." -ForegroundColor Yellow
        docker-compose -f $composeFile down
        Write-Host "Cleanup complete." -ForegroundColor Green
    }
    
    "clean-all" {
        if (-not (Test-DockerRunning)) { exit 1 }
        Write-Host "WARNING: This will remove all containers, volumes, and images!" -ForegroundColor Red
        $confirm = Read-Host "Are you sure? (y/N)"
        if ($confirm -eq "y" -or $confirm -eq "Y") {
            docker-compose down -v --rmi all --remove-orphans
            docker system prune -f
            Write-Host "Complete cleanup finished." -ForegroundColor Green
        } else {
            Write-Host "Cleanup cancelled." -ForegroundColor Yellow
        }
    }
    
    "backup-db" {
        if (-not (Test-DockerRunning)) { exit 1 }
        $composeFile = Get-ActiveComposeFile
        
        $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
        $defaultBackup = "backup_$timestamp.sql"
        
        if ([string]::IsNullOrEmpty($BackupFile)) {
            $BackupFile = $defaultBackup
        }
        
        Write-Host "Creating database backup to $BackupFile..." -ForegroundColor Blue
        try {
            docker-compose -f $composeFile exec postgres pg_dump -U desco_user desco_report_db > $BackupFile
            Write-Host "Backup created successfully: $BackupFile" -ForegroundColor Green
        } catch {
            Write-Host "Backup failed: $_" -ForegroundColor Red
        }
    }
    
    "restore-db" {
        if (-not (Test-DockerRunning)) { exit 1 }
        if ([string]::IsNullOrEmpty($RestoreFile)) {
            Write-Host "Please specify backup file with -RestoreFile parameter" -ForegroundColor Red
            exit 1
        }
        
        if (-not (Test-Path $RestoreFile)) {
            Write-Host "Backup file not found: $RestoreFile" -ForegroundColor Red
            exit 1
        }
        
        $composeFile = Get-ActiveComposeFile
        Write-Host "Restoring database from $RestoreFile..." -ForegroundColor Yellow
        
        try {
            docker-compose -f $composeFile exec -T postgres psql -U desco_user -d desco_report_db < $RestoreFile
            Write-Host "Database restored successfully." -ForegroundColor Green
        } catch {
            Write-Host "Restore failed: $_" -ForegroundColor Red
        }
    }
    
    "shell" {
        if (-not (Test-DockerRunning)) { exit 1 }
        $composeFile = Get-ActiveComposeFile
        Write-Host "Opening application shell..." -ForegroundColor Blue
        docker-compose -f $composeFile exec desco-server /bin/bash
    }
    
    "db-shell" {
        if (-not (Test-DockerRunning)) { exit 1 }
        $composeFile = Get-ActiveComposeFile
        Write-Host "Opening PostgreSQL shell..." -ForegroundColor Blue
        docker-compose -f $composeFile exec postgres psql -U desco_user -d desco_report_db
    }
    
    "redis-shell" {
        if (-not (Test-DockerRunning)) { exit 1 }
        $composeFile = Get-ActiveComposeFile
        Write-Host "Opening Redis CLI..." -ForegroundColor Blue
        docker-compose -f $composeFile exec redis redis-cli
    }
    
    default {
        Show-Help
    }
}