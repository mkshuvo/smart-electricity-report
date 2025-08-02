# DESC Report Server Makefile
# Cross-platform Makefile for Docker/Podman support
# Works on Windows (PowerShell), Linux, and macOS

# Detect container runtime (Docker or Podman)
CONTAINER_RUNTIME := $(shell which docker 2>/dev/null || which podman 2>/dev/null || echo "docker")

# Default values
PROJECT_NAME := desco-report-server
DOCKER_COMPOSE := $(CONTAINER_RUNTIME)-compose
DOCKER := $(CONTAINER_RUNTIME)

# Environment detection
ifeq ($(OS),Windows_NT)
    # Windows specific
    SHELL := cmd
    RM := del /Q
    MKDIR := mkdir
    SEP := \\
    ENV_FILE := .env
    DEV_UP := $(DOCKER_COMPOSE) -f docker-compose.yml up -d
    PROD_UP := $(DOCKER_COMPOSE) -f docker-compose.prod.yml up -d
    DOWN := $(DOCKER_COMPOSE) down
    LOGS := $(DOCKER_COMPOSE) logs -f
    CLEAN := $(DOCKER_COMPOSE) down -v --remove-orphans
else
    # Unix-like systems (Linux/macOS)
    SHELL := /bin/bash
    RM := rm -rf
    MKDIR := mkdir -p
    SEP := /
    ENV_FILE := .env
    DEV_UP := $(DOCKER_COMPOSE) -f docker-compose.yml up -d
    PROD_UP := $(DOCKER_COMPOSE) -f docker-compose.prod.yml up -d
    DOWN := $(DOCKER_COMPOSE) down
    LOGS := $(DOCKER_COMPOSE) logs -f
    CLEAN := $(DOCKER_COMPOSE) down -v --remove-orphans
endif

# Colors for output (Unix only)
ifneq ($(OS),Windows_NT)
    RED := \033[0;31m
    GREEN := \033[0;32m
    YELLOW := \033[0;33m
    BLUE := \033[0;34m
    NC := \033[0m # No Color
else
    RED :=
    GREEN :=
    YELLOW :=
    BLUE :=
    NC :=
endif

.PHONY: help build dev prod stop restart logs clean clean-all init test health status

# Default target
help:
	@echo "$(GREEN)DESC Report Server - Container Management$(NC)"
	@echo ""
	@echo "$(BLUE)Container Runtime: $(CONTAINER_RUNTIME)$(NC)"
	@echo ""
	@echo "$(YELLOW)Development Commands:$(NC)"
	@echo "  make dev       - Start development environment"
	@echo "  make dev-up    - Start development services"
	@echo "  make dev-down  - Stop development services"
	@echo ""
	@echo "$(YELLOW)Production Commands:$(NC)"
	@echo "  make prod      - Start production environment"
	@echo "  make prod-up   - Start production services"
	@echo "  make prod-down - Stop production services"
	@echo ""
	@echo "$(YELLOW)Utility Commands:$(NC)"
	@echo "  make build     - Build Docker images"
	@echo "  make restart   - Restart all services"
	@echo "  make logs      - View logs from all services"
	@echo "  make logs-app  - View only application logs"
	@echo "  make health    - Check health of all services"
	@echo "  make status    - Show container status"
	@echo ""
	@echo "$(YELLOW)Cleanup Commands:$(NC)"
	@echo "  make clean     - Stop and remove containers"
	@echo "  make clean-all - Clean everything including volumes"
	@echo ""
	@echo "$(YELLOW)Setup Commands:$(NC)"
	@echo "  make init      - Initialize environment and dependencies"
	@echo "  make env       - Create .env file from template"
	@echo ""

# Development environment
dev: env dev-up
	@echo "$(GREEN)Development environment started!$(NC)"
	@echo "$(BLUE)API available at: http://localhost:8080$(NC)"
	@echo "$(BLUE)PostgreSQL available at: localhost:5432$(NC)"
	@echo "$(BLUE)Redis available at: localhost:6379$(NC)"

dev-up:
	$(DEV_UP)

dev-down:
	$(DOWN)

# Production environment
prod: env prod-up
	@echo "$(GREEN)Production environment started!$(NC)"
	@echo "$(BLUE)API available at: http://localhost:8080$(NC)"
	@echo "$(YELLOW)Remember to set secure passwords in .env file!$(NC)"

prod-up:
	$(PROD_UP)

prod-down:
	$(DOCKER_COMPOSE) -f docker-compose.prod.yml down

# Build images
build:
	@echo "$(BLUE)Building Docker images...$(NC)"
	$(DOCKER_COMPOSE) build

build-prod:
	@echo "$(BLUE)Building production Docker images...$(NC)"
	$(DOCKER_COMPOSE) -f docker-compose.prod.yml build

# Restart services
restart: stop dev-up

# Stop services
stop:
	$(DOWN)

# Logs
logs:
	$(LOGS)

logs-app:
	$(DOCKER_COMPOSE) logs -f desco-server

# Health checks
health:
	@echo "$(BLUE)Checking service health...$(NC)"
	$(DOCKER) ps
	$(DOCKER_COMPOSE) ps

# Status
status:
	@echo "$(GREEN)Container Status:$(NC)"
	$(DOCKER_COMPOSE) ps
	@echo ""
	@echo "$(GREEN)Image Status:$(NC)"
	$(DOCKER) images | grep desco

# Cleanup
clean:
	@echo "$(YELLOW)Stopping and removing containers...$(NC)"
	$(CLEAN)

clean-all: clean
	@echo "$(RED)Removing all volumes and images...$(NC)"
	$(DOCKER_COMPOSE) down -v --rmi all --remove-orphans
	$(DOCKER) system prune -f

# Environment setup
env:
	@if [ ! -f $(ENV_FILE) ]; then \
		echo "Creating .env file..."; \
		cp .env.example .env || echo "Creating new .env file..."; \
		$(MAKE) create-env; \
	fi

# Windows-specific commands
ifeq ($(OS),Windows_NT)
create-env:
	@echo POSTGRES_USER=desco_user > .env
	@echo POSTGRES_PASSWORD=desco_password >> .env
	@echo POSTGRES_DB=desco_report_db >> .env
	@echo REDIS_PASSWORD=redis_password >> .env
	@echo JWT_SECRET_KEY=your-super-secret-jwt-key-change-this-in-production >> .env
else
create-env:
	@echo "POSTGRES_USER=desco_user" > .env
	@echo "POSTGRES_PASSWORD=desco_password" >> .env
	@echo "POSTGRES_DB=desco_report_db" >> .env
	@echo "REDIS_PASSWORD=redis_password" >> .env
	@echo "JWT_SECRET_KEY=your-super-secret-jwt-key-change-this-in-production" >> .env
endif

# Database commands
db-migrate:
	@echo "$(BLUE)Running database migrations...$(NC)"
	$(DOCKER_COMPOSE) exec desco-server dotnet ef database update

db-reset:
	@echo "$(RED)Resetting database...$(NC)"
	$(DOCKER_COMPOSE) down postgres
	$(DOCKER) volume rm desco-report-server_postgres_data || true
	$(DOCKER_COMPOSE) up -d postgres

# Development utilities
dev-shell:
	$(DOCKER_COMPOSE) exec desco-server /bin/bash

dev-db-shell:
	$(DOCKER_COMPOSE) exec postgres psql -U desco_user -d desco_report_db

dev-redis-shell:
	$(DOCKER_COMPOSE) exec redis redis-cli

# Test commands
test:
	@echo "$(BLUE)Running tests...$(NC)"
	$(DOCKER_COMPOSE) exec desco-server dotnet test

# Security scan
scan:
	@echo "$(BLUE)Scanning for vulnerabilities...$(NC)"
	$(DOCKER) run --rm -v /var/run/docker.sock:/var/run/docker.sock \
		aquasec/trivy:latest image desco-report-server_desco-server:latest

# Backup commands
backup-db:
	@echo "$(BLUE)Creating database backup...$(NC)"
	$(DOCKER_COMPOSE) exec postgres pg_dump -U desco_user desco_report_db > backup_$(shell date +%Y%m%d_%H%M%S).sql

restore-db:
	@echo "$(YELLOW)Restoring database from backup...$(NC)"
	$(DOCKER_COMPOSE) exec -T postgres psql -U desco_user -d desco_report_db < $(file)

# Update dependencies
update:
	@echo "$(BLUE)Updating dependencies...$(NC)"
	$(DOCKER_COMPOSE) pull
	$(DOCKER_COMPOSE) build --no-cache