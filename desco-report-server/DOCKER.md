# DESC Report Server - Docker Setup Guide

This guide provides comprehensive instructions for running the DESC Report Server using Docker or Podman across different platforms.

## Prerequisites

- **Docker Desktop** (Windows/macOS) or **Docker Engine** (Linux)
- **Docker Compose** v2.0+
- **Make** (optional, for convenience commands)
- **PowerShell** (Windows) or **Bash** (Linux/macOS)

## Quick Start

### 1. Environment Setup

Copy the environment template and configure:

```bash
# Unix/Linux/macOS
cp .env.example .env

# Windows PowerShell
copy .env.example .env
```

Edit `.env` file with your specific configuration:
- Database passwords
- JWT secret key
- Email settings
- API configurations

### 2. Development Environment

#### Using Make (Recommended)

```bash
# Start development environment
make dev

# View logs
make logs

# Stop environment
make dev-down
```

#### Manual Commands

```bash
# Build and start services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

### 3. Production Environment

#### Using Make

```bash
# Start production environment
make prod

# Build production images
make build-prod

# View production logs
make logs
```

#### Manual Commands

```bash
# Build production images
docker-compose -f docker-compose.prod.yml build

# Start production services
docker-compose -f docker-compose.prod.yml up -d

# Stop production services
docker-compose -f docker-compose.prod.yml down
```

## Platform-Specific Instructions

### Windows (PowerShell)

#### Option 1: Using Make (if available)
```powershell
# Install Make (if not available)
# Using Chocolatey: choco install make
# Using Scoop: scoop install make

make dev
```

#### Option 2: PowerShell Scripts
```powershell
# Development
.\scripts\dev-start.ps1
.\scripts\dev-stop.ps1
.\scripts\dev-logs.ps1

# Production
.\scripts\prod-start.ps1
.\scripts\prod-stop.ps1
```

#### Option 3: Direct Docker Commands
```powershell
# Development
docker-compose up -d
docker-compose logs -f desco-server

# Production
docker-compose -f docker-compose.prod.yml up -d
```

### Linux/macOS

#### Using Make
```bash
# Development
make dev

# Production
make prod

# Utility commands
make health
make status
make clean
```

#### Manual Commands
```bash
# Development
docker-compose up -d

# Production
docker-compose -f docker-compose.prod.yml up -d
```

## Podman Support

The project supports both Docker and Podman automatically:

```bash
# Podman will be detected automatically
make dev

# Manual Podman commands
podman-compose up -d
```

## Service URLs

After starting the services:

- **DESC API**: http://localhost:8080
- **PostgreSQL**: localhost:5432
- **Redis**: localhost:6379
- **Health Check**: http://localhost:8080/health

## Database Management

### Backup Database

```bash
# Using Make
make backup-db

# Manual backup
docker-compose exec postgres pg_dump -U desco_user desco_report_db > backup.sql
```

### Restore Database

```bash
# Using Make
make restore-db file=backup.sql

# Manual restore
docker-compose exec -T postgres psql -U desco_user -d desco_report_db < backup.sql
```

### Database Shell Access

```bash
# Using Make
make dev-db-shell

# Manual access
docker-compose exec postgres psql -U desco_user -d desco_report_db
```

## Redis Management

### Redis CLI Access

```bash
# Using Make
make dev-redis-shell

# Manual access
docker-compose exec redis redis-cli
```

## Monitoring and Debugging

### Health Checks

```bash
# Using Make
make health

# Manual health check
docker-compose ps
curl http://localhost:8080/health
```

### View Logs

```bash
# All services
make logs

# Specific service
make logs-app

# Manual logs
docker-compose logs -f desco-server
```

### Container Status

```bash
# Using Make
make status

# Manual status
docker-compose ps
docker images | grep desco
```

## Security Considerations

### Production Deployment

1. **Change default passwords** in `.env`
2. **Use strong JWT secret** (minimum 32 characters)
3. **Enable HTTPS** with proper certificates
4. **Configure firewall** rules
5. **Use secrets management** for sensitive data

### Environment Variables

Critical variables to change in production:

```bash
# Database
POSTGRES_PASSWORD=your-secure-password

# Redis
REDIS_PASSWORD=your-secure-redis-password

# JWT
JWT_SECRET_KEY=your-very-long-secure-jwt-key-minimum-32-chars

# Email
SMTP_PASSWORD=your-email-app-password
```

## Troubleshooting

### Common Issues

#### Port Already in Use
```bash
# Check what's using port 8080
# Windows: netstat -ano | findstr :8080
# Linux/macOS: lsof -i :8080

# Change ports in docker-compose.yml
```

#### Permission Denied (Linux/macOS)
```bash
# Fix Docker permissions
sudo usermod -aG docker $USER
# Logout and login again
```

#### Database Connection Issues
```bash
# Check if PostgreSQL is ready
docker-compose exec postgres pg_isready -U desco_user

# Check logs
docker-compose logs postgres
```

#### Build Failures
```bash
# Clean build cache
docker-compose build --no-cache

# Clean all
make clean-all
```

### Performance Tuning

#### Development
```yaml
# In docker-compose.yml
services:
  desco-server:
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    volumes:
      - .:/app  # For hot reload
```

#### Production
```yaml
# In docker-compose.prod.yml
services:
  desco-server:
    deploy:
      resources:
        limits:
          memory: 1G
          cpus: '1.0'
```

## Cleanup

### Remove All Containers and Volumes

```bash
# Using Make
make clean-all

# Manual cleanup
docker-compose down -v --rmi all --remove-orphans
docker system prune -f
```

### Reset Everything

```bash
# Complete reset
make clean-all
rm -rf .env
# Then re-run setup
```

## Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [PostgreSQL Docker Image](https://hub.docker.com/_/postgres)
- [Redis Docker Image](https://hub.docker.com/_/redis)
- [ASP.NET Core Docker](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/)

## Support

For issues or questions:
1. Check the troubleshooting section
2. Review container logs: `make logs`
3. Check service health: `make health`
4. Verify environment configuration in `.env`