@echo off
REM DESC Report Server - Windows Batch Script for Docker Management
REM Usage: scripts\run.bat [command]

echo DESC Report Server - Docker Management

if "%1"=="" goto :help
if "%1"=="help" goto :help
if "%1"=="dev" goto :dev
if "%1"=="prod" goto :prod
if "%1"=="build" goto :build
if "%1"=="logs" goto :logs
if "%1"=="clean" goto :clean
if "%1"=="status" goto :status
if "%1"=="health" goto :health

:help
echo.
echo Usage: scripts\run.bat [command]
echo.
echo Commands:
echo   dev      - Start development environment
echo   prod     - Start production environment  
echo   build    - Build Docker images
echo   logs     - View container logs
echo   clean    - Stop and remove containers
echo   status   - Show container status
echo   health   - Check service health
echo   help     - Show this help message
echo.
goto :eof

:dev
echo Starting development environment...
if not exist .\.env (
    echo Creating .env file...
    echo POSTGRES_USER=desco_user > .\.env
    echo POSTGRES_PASSWORD=desco_password >> .\.env
    echo POSTGRES_DB=desco_report_db >> .\.env
    echo REDIS_PASSWORD=redis_password >> .\.env
    echo JWT_SECRET_KEY=your-super-secret-jwt-key-change-this-in-production >> .\.env
    echo Please update .env with your configuration.
)
docker-compose up -d
echo Development environment started!
echo API: http://localhost:8080
goto :eof

:prod
echo Starting production environment...
if not exist .\.env (
    echo .env file not found. Creating from template...
    copy .\.env.example .\.env
    echo Please update .env with production values before continuing.
    pause
)
docker-compose -f docker-compose.prod.yml up -d
echo Production environment started!
echo API: http://localhost:8080
goto :eof

:build
echo Building Docker images...
docker-compose build
goto :eof

:logs
echo Viewing container logs...
docker-compose logs -f
goto :eof

:clean
echo Stopping and cleaning containers...
docker-compose down
echo Cleanup complete.
goto :eof

:status
echo Container Status:
docker-compose ps
goto :eof

:health
echo Service Health Check:
docker-compose ps
echo.
echo Docker Images:
docker images ^| findstr desco
goto :eof