# DESC Report Server - Project Plan

## Project Overview
A .NET 8.0 Web API backend server for the DESC Report system that provides cached electricity consumption data from the Bangladesh Electricity Distribution Company (DESC).

## Architecture Overview
- **Backend**: .NET 8.0 Web API
- **Database**: PostgreSQL
- **Caching**: Redis
- **Background Jobs**: Cron-based data fetching
- **Frontend Integration**: REST API for Next.js UI

## Project Structure
```
desco-report-server/
├── Controllers/
│   ├── AuthController.cs
│   ├── UsersController.cs
│   ├── DescoAccountsController.cs
│   ├── ReportsController.cs
│   └── HealthController.cs
├── Models/
│   ├── User.cs
│   ├── DescoAccount.cs
│   ├── ConsumptionReport.cs
│   └── BalanceReport.cs
├── Services/
│   ├── AuthService.cs
│   ├── DescoDataService.cs
│   ├── CacheService.cs
│   ├── BackgroundJobService.cs
│   └── EmailService.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   └── Repositories/
├── Migrations/
├── Jobs/
│   └── ReportFetcherJob.cs
├── Configurations/
├── appsettings.json
└── Program.cs
```

## Core Features

### 1. User Management
- **Registration**: Full name, phone number, NID, email, password
- **Authentication**: JWT tokens
- **Authorization**: Role-based access
- **Password Reset**: Email-based

### 2. DESC Account Management
- **Account Registration**: Link DESC accounts using account number
- **Meter Number Discovery**: Auto-fetch meter number from account number
- **Multiple Accounts**: Users can add multiple DESC accounts
- **Account Validation**: Verify account ownership

### 3. Data Management
- **Consumption Reports**: Daily consumption data
- **Balance Reports**: Current balance and payment status
- **Historical Data**: Store historical consumption patterns
- **Data Validation**: Ensure data integrity

### 4. Caching Strategy
- **Redis Cache**: Store frequently accessed data
- **Cache TTL**: 1 hour for live data, 24 hours for historical
- **Cache Invalidation**: Manual and automatic refresh
- **Performance**: Reduce DESC server load

### 5. Background Jobs
- **Cron Schedule**: Fetch reports every 6 hours
- **Data Sync**: Update existing records
- **Error Handling**: Retry mechanisms and logging
- **Monitoring**: Job success/failure notifications

## Database Schema

### Users Table
```sql
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    full_name VARCHAR(100) NOT NULL,
    phone_number VARCHAR(20) UNIQUE NOT NULL,
    nid VARCHAR(20) UNIQUE NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    is_verified BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE,
    failed_login_attempts INTEGER DEFAULT 0,
    lockout_end_time TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

### DescoAccounts Table
```sql
CREATE TABLE desco_accounts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    account_number VARCHAR(50) NOT NULL,
    meter_number VARCHAR(50),
    customer_name VARCHAR(100),
    installation_address TEXT,
    phase_type VARCHAR(20),
    sanction_load VARCHAR(20),
    tariff_solution VARCHAR(50),
    is_active BOOLEAN DEFAULT TRUE,
    is_verified BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(user_id, account_number)
);
```

### ConsumptionReports Table
```sql
CREATE TABLE consumption_reports (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    desco_account_id UUID REFERENCES desco_accounts(id) ON DELETE CASCADE,
    date DATE NOT NULL,
    consumed_taka DECIMAL(10,2),
    consumed_unit DECIMAL(10,2),
    balance DECIMAL(10,2),
    reading_time TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(desco_account_id, date)
);
```

### UserSessions Table
```sql
CREATE TABLE user_sessions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    refresh_token VARCHAR(500) NOT NULL,
    expires_at TIMESTAMP NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_revoked BOOLEAN DEFAULT FALSE
);
```

## API Endpoints

### Authentication
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `POST /api/auth/logout` - User logout
- `POST /api/auth/refresh` - Refresh token
- `POST /api/auth/forgot-password` - Password reset
- `POST /api/auth/reset-password` - Reset password with token

### User Management
- `GET /api/users/profile` - Get user profile
- `PUT /api/users/profile` - Update user profile
- `DELETE /api/users/account` - Delete account
- `POST /api/users/verify-email` - Email verification
- `PUT /api/users/change-password` - Change password

### DESC Accounts
- `GET /api/desco-accounts` - Get user's DESC accounts
- `POST /api/desco-accounts` - Add new DESC account
- `PUT /api/desco-accounts/{id}` - Update account details
- `DELETE /api/desco-accounts/{id}` - Remove account
- `POST /api/desco-accounts/{id}/verify` - Verify account ownership

### Reports
- `GET /api/reports/consumption/{accountId}` - Get consumption reports
- `GET /api/reports/balance/{accountId}` - Get current balance
- `GET /api/reports/summary/{accountId}` - Get summary statistics
- `GET /api/reports/download/{accountId}` - Download report as CSV/PDF

## Configuration Requirements

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=desco_report_db;Username=postgres;Password=your_password",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "Key": "your_secret_key_here_min_32_chars_for_security",
    "Issuer": "desco-report-server",
    "Audience": "desco-report-ui",
    "ExpireDays": 7,
    "RefreshExpireDays": 30
  },
  "Desco": {
    "BaseUrl": "https://desco.org.bd",
    "ApiTimeout": 30,
    "RetryCount": 3,
    "RetryDelay": 2000
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "Username": "your_email@gmail.com",
    "Password": "your_app_password",
    "FromEmail": "noreply@descoreport.com",
    "FromName": "DESC Report System"
  },
  "RateLimit": {
    "RequestsPerMinute": 60,
    "RequestsPerHour": 1000,
    "LoginAttempts": 5,
    "LockoutDuration": 15
  },
  "CronJobs": {
    "ReportFetchInterval": "0 */6 * * *",
    "CacheCleanupInterval": "0 2 * * *",
    "EmailCleanupInterval": "0 3 * * *"
  }
}
```

## Development Phases

### Phase 1: Project Setup & Docker Configuration
- [x] Install required NuGet packages
- [x] **Docker Support Setup**
  - [x] **Multi-stage Dockerfile** - Optimized for production with security best practices
    - [x] Multi-stage build (build, publish, runtime stages)
    - [x] Non-root user for security (user ID 1001)
    - [x] Health checks with curl
    - [x] Minimal Alpine-based runtime
    - [x] Security hardening with read-only filesystem
  - [x] **Docker Compose Configuration**
    - [x] **Development Environment** (`docker-compose.yml`)
      - PostgreSQL 15 with health checks
      - Redis 7 with persistence
      - DESC server with hot reload support
      - Named volumes for data persistence
      - Development-specific configurations
    - [x] **Production Environment** (`docker-compose.prod.yml`)
      - Resource limits and security constraints
      - Production-grade PostgreSQL with SSL
      - Redis with authentication and persistence
      - Environment variable validation
      - Resource monitoring and health checks
  - [x] **Cross-platform Makefile** with Docker/Podman support
    - [x] **Windows Compatibility** - PowerShell commands
    - [x] **Linux/macOS Compatibility** - Bash commands
    - [x] **Podman Support** - Automatic detection and usage
    - [x] **Comprehensive Commands**:
      - `make dev` - Start development environment
      - `make prod` - Start production environment
      - `make build` - Build Docker images
      - `make logs` - View container logs
      - `make health` - Check service health
      - `make clean` - Cleanup containers and volumes
      - `make backup-db` - Database backup functionality
      - `make restore-db` - Database restore functionality
  - [x] **Environment Configuration**
    - [x] `.env.example` - Template with all required variables
    - [x] `.dockerignore` - Optimized build context
    - [x] **Security Configuration**:
      - JWT secret key management
      - Database password encryption
      - Redis authentication
      - CORS configuration
- [ ] Set up PostgreSQL database with proper schema
- [ ] Configure Redis server connection
- [ ] Create initial database migrations
- [ ] Set up Serilog logging configuration
- [ ] Configure CORS for frontend integration
- [ ] Set up dependency injection container

### Phase 2: User Authentication & Registration System

#### 2.1 Database Setup
- [ ] Create ApplicationDbContext with entity configurations
- [ ] Design User entity with Identity Framework integration
- [ ] Create initial migration for user management
- [ ] Set up database seeding for roles and admin user
- [ ] Configure connection string encryption

#### 2.2 User Registration Flow
- [ ] **Registration Endpoint**: `/api/auth/register`
  - Request validation (full name, phone, NID, email, password)
  - NID format validation (Bangladesh NID format)
  - Phone number validation (Bangladesh mobile format)
  - Email uniqueness check
  - Password strength requirements (min 8 chars, uppercase, lowercase, number, special char)
  - Account creation with email verification token
  - Send welcome email with verification link

- [ ] **Email Verification**: `/api/users/verify-email`
  - Generate unique verification token
  - Store token with expiration (24 hours)
  - Send verification email with secure link
  - Handle token validation and user activation

#### 2.3 Authentication System
- [ ] **Login Endpoint**: `/api/auth/login`
  - Email/Phone number authentication
  - Password verification with BCrypt
  - Failed login attempt tracking
  - Account lockout mechanism (15 min after 5 attempts)
  - Generate JWT access token (7 days expiry)
  - Generate refresh token (30 days expiry)
  - Store refresh token in database

- [ ] **Token Management**: `/api/auth/refresh`
  - Refresh token validation
  - Generate new access token
  - Rotate refresh token for security
  - Revoke old refresh tokens

- [ ] **Password Management**
  - Forgot password: Generate reset token (1 hour expiry)
  - Reset password: Validate token and update password
  - Change password: Verify old password, update with new
  - Password history tracking (prevent reuse of last 5 passwords)

#### 2.4 User Profile Management
- [ ] **Profile Management**
  - Get user profile with DESC accounts count
  - Update profile (full name, phone, email)
  - Email change verification process
  - Phone number change verification via OTP
  - Account deletion with data retention policy

#### 2.5 Security Implementation
- [ ] **Security Features**
  - JWT token validation middleware
  - Role-based authorization (User, Admin)
  - API rate limiting per user/IP
  - HTTPS enforcement in production
  - Security headers (CSP, HSTS, X-Frame-Options)
  - Input sanitization and validation
  - SQL injection prevention with parameterized queries
  - XSS protection

### Phase 3: DESC Account Management System

#### 3.1 Account Registration
- [ ] **Add DESC Account**: `/api/desco-accounts`
  - Validate account number format (DESC specific)
  - Check for existing account under user
  - Fetch meter details from DESC API
  - Store account information with user association
  - Set verification status to pending

- [ ] **Account Verification**: `/api/desco-accounts/{id}/verify`
  - Send verification SMS to registered phone
  - OTP-based verification (6-digit code)
  - Cross-reference with DESC database
  - Update verification status on success

#### 3.2 Account Management
- [ ] **Account Operations**
  - List user's DESC accounts with verification status
  - Update account nickname/custom name
  - Deactivate/reactivate accounts
  - Set primary account preference
  - Account sharing permissions (future feature)

#### 3.3 Data Synchronization
- [ ] **Background Data Fetching**
  - Schedule job every 6 hours using Hangfire
  - Fetch consumption data for all active accounts
  - Handle API rate limits and retries
  - Parse and validate DESC response format
  - Store historical data with proper indexing

### Phase 4: Data Integration & Processing

#### 4.1 DESC Data Service
- [ ] **Data Fetching Service**
  - Implement DESC API integration with retry logic
  - Handle authentication with DESC system
  - Parse HTML/XML response from DESC
  - Extract consumption, balance, and meter readings
  - Handle different tariff types and billing cycles

#### 4.2 Data Processing Pipeline
- [ ] **Data Processing**
  - Normalize date formats and timezones
  - Calculate daily consumption differences
  - Handle missing data scenarios
  - Detect anomalies in consumption patterns
  - Generate alerts for unusual consumption

#### 4.3 Caching Strategy
- [ ] **Redis Implementation**
  - Cache user profiles and account data (1 hour TTL)
  - Cache recent consumption data (30 minutes TTL)
  - Cache historical summaries (6 hours TTL)
  - Implement cache warming for active users
  - Handle cache invalidation on data updates

### Phase 5: API Development & Documentation

#### 5.1 RESTful API Design
- [ ] **API Standards**
  - RESTful conventions with proper HTTP methods
  - Consistent response formats (success/error)
  - Pagination for large datasets
  - Filtering and sorting capabilities
  - API versioning strategy

#### 5.2 Response Models
- [ ] **Standardized Responses**
  - Success response wrapper with metadata
  - Error response with detailed error codes
  - Validation error formatting
  - Rate limit headers
  - Caching headers

#### 5.3 API Documentation
- [ ] **Documentation Setup**
  - Swagger/OpenAPI 3.0 integration
  - Detailed endpoint descriptions
  - Request/response examples
  - Authentication guide
  - Rate limit documentation

### Phase 6: Testing & Quality Assurance

#### 6.1 Unit Testing
- [ ] **Test Coverage**
  - Service layer unit tests (90% coverage)
  - Repository pattern tests with in-memory database
  - Authentication service tests
  - DESC data parsing tests
  - Validation logic tests

#### 6.2 Integration Testing
- [ ] **End-to-End Testing**
  - API endpoint integration tests
  - Database transaction tests
  - Redis caching integration tests
  - Background job execution tests
  - DESC API mock integration

#### 6.3 Security Testing
- [ ] **Security Validation**
  - JWT token security tests
  - SQL injection prevention tests
  - XSS protection tests
  - Rate limiting effectiveness tests
  - Authentication bypass attempts

### Phase 7: Performance & Monitoring

#### 7.1 Performance Optimization
- [ ] **Database Optimization**
  - Indexing strategy for user queries
  - Query optimization with execution plans
  - Connection pooling configuration
  - Database partitioning for large datasets
  - Read replicas for reporting queries

#### 7.2 Monitoring Setup
- [ ] **Application Monitoring**
  - Application Insights integration
  - Custom metrics for API usage
  - Error tracking and alerting
  - Performance bottleneck identification
  - Database query performance monitoring

#### 7.3 Logging Strategy
- [ ] **Comprehensive Logging**
  - Structured logging with Serilog
  - Request/response logging (excluding sensitive data)
  - Authentication audit logs
  - Background job execution logs
  - DESC API interaction logs

### Phase 8: Deployment & DevOps

#### 8.1 Containerization
- [ ] **Docker Setup**
  - Multi-stage Dockerfile for optimized builds
  - Docker Compose for local development
  - Environment-specific configurations
  - Health check endpoints
  - Graceful shutdown handling

#### 8.2 CI/CD Pipeline
- [ ] **Deployment Pipeline**
  - GitHub Actions workflow
  - Automated testing on pull requests
  - Security scanning in pipeline
  - Database migration automation
  - Staging environment deployment

#### 8.3 Production Readiness
- [ ] **Production Setup**
  - Environment variable configuration
  - SSL/TLS certificate setup
  - Database backup strategy
  - Redis persistence configuration
  - Load balancer configuration

## Security Considerations

### Data Protection
- **Password Storage**: BCrypt hashing with work factor 12
- **Sensitive Data**: Encryption at rest for NID and phone numbers
- **PII Handling**: Data retention policy (90 days after account deletion)
- **Audit Trail**: All authentication events logged
- **GDPR Compliance**: Right to be forgotten implementation

### API Security
- **Rate Limiting**: 60 requests/minute, 1000/hour per user
- **CORS**: Whitelist specific frontend origins
- **Content Security**: Input validation using FluentValidation
- **SQL Injection**: Entity Framework Core parameterized queries
- **XSS Prevention**: Output encoding for all responses

### Infrastructure Security
- **Database Security**: Connection string encryption
- **Redis Security**: Password authentication and SSL
- **Secrets Management**: Azure Key Vault integration
- **Network Security**: Firewall rules and VPN access
- **Monitoring**: Failed login attempt alerts

## Performance Optimization

### Database Optimization
- **Indexing**: Composite indexes on (user_id, created_at)
- **Partitioning**: Monthly partitioning for consumption reports
- **Caching**: Second-level caching with Redis
- **Connection Pooling**: Npgsql connection pooling
- **Query Optimization**: Include statements for related data

### API Performance
- **Response Compression**: Gzip/Brotli compression
- **Caching Headers**: ETag and Cache-Control
- **Pagination**: Cursor-based pagination for large datasets
- **Async Operations**: Async/await throughout the codebase
- **Background Processing**: Hangfire for long-running tasks

### Caching Strategy
- **User Sessions**: 7 days TTL for JWT tokens
- **Account Data**: 1 hour TTL with sliding expiration
- **Consumption Data**: 30 minutes TTL for recent data
- **Historical Data**: 6 hours TTL for aggregated data
- **Cache Warming**: Preload active user data

## Monitoring & Logging

### Application Insights
- **Custom Events**: User registration, login, account linking
- **Metrics**: API response times, database query performance
- **Dependencies**: DESC API response times and failure rates
- **Exceptions**: Structured exception logging with context
- **Availability**: Health check monitoring every 5 minutes

### Serilog Configuration
```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.File", "Serilog.Sinks.Console"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "logs/desco-report-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      },
      {
        "Name": "Console"
      }
    ]
  }
}
```

### Health Checks
- **Database Connectivity**: PostgreSQL connection test
- **Redis Availability**: Cache connectivity check
- **DESC API Health**: External service availability
- **Disk Space**: Log file storage monitoring
- **Memory Usage**: Application memory consumption

## Next Steps
1. **Immediate Actions**:
   - Create ApplicationDbContext with Identity integration
   - Set up User and IdentityRole entities
   - Configure JWT authentication in Program.cs
   - Create initial migration for user management

2. **Week 1 Focus**:
   - Implement user registration endpoint with validation
   - Set up email service for verification
   - Create authentication middleware
   - Build login/logout functionality

3. **Week 2 Focus**:
   - Implement DESC account linking
   - Create background job for data fetching
   - Set up Redis caching layer
   - Build user profile management

4. **Week 3 Focus**:
   - Complete API endpoints for reports
   - Implement comprehensive testing
   - Set up monitoring and logging
   - Prepare for production deployment

This comprehensive plan provides detailed implementation guidance for user registration, authentication, and data management systems in the DESC Report Server.