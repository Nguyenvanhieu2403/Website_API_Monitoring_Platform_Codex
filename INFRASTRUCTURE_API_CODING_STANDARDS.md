# Infrastructure, API Standards, and Coding Standards

## 1. Infrastructure Diagram

### Multi-Region, High-Availability Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          CDN LAYER (CloudFront)                             │
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐                 │
│  │  Static Assets │  │  JS/CSS Bundles│  │  API Caching   │                 │
│  │  (4h TTL)      │  │  (1h TTL)      │  │  (5min TTL)    │                 │
│  └────────────────┘  └────────────────┘  └────────────────┘                 │
└────────────────────────────────┬──────────────────────────────────────────┘
                                 │
┌────────────────────────────────────────────────────────────────────────────┐
│                  GLOBAL LOAD BALANCER (Route 53)                           │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │ Health Checks | Geo-routing | Failover | Weighted policies          │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
└────────────────────────────────┬──────────────────────────────────────────┘
                                 │
        ┌────────────────────────┼────────────────────────┐
        │                        │                        │
        ▼                        ▼                        ▼
┌────────────────────────┐ ┌────────────────────────┐ ┌────────────────────────┐
│  PRIMARY REGION        │ │  SECONDARY REGION      │ │  TERTIARY REGION       │
│  (US-EAST-1)          │ │  (EU-WEST-1)          │ │  (APAC-SE-1)          │
├────────────────────────┤ ├────────────────────────┤ ├────────────────────────┤
│                        │ │                        │ │                        │
│ ┌──────────────────┐   │ │ ┌──────────────────┐   │ │ ┌──────────────────┐   │
│ │  ALB (Public)    │   │ │ │  ALB (Public)    │   │ │ │  ALB (Public)    │   │
│ │  Port: 80, 443   │   │ │ │  Port: 80, 443   │   │ │ │  Port: 80, 443   │   │
│ └────────┬─────────┘   │ │ └────────┬─────────┘   │ │ └────────┬─────────┘   │
│          │             │ │          │             │ │          │             │
│ ┌────────────────────┐ │ │ ┌────────────────────┐ │ │ ┌────────────────────┐ │
│ │ API Container Cluster  │ │ API Container Cluster  │ │ API Container Cluster  │
│ │ (ECS/EKS)            │ │ (ECS/EKS)            │ │ (ECS/EKS)            │
│ │                      │ │                      │ │                      │
│ │  ┌─────────────────┐ │ │ ┌─────────────────┐ │ │ ┌─────────────────┐ │
│ │  │  API Server 1   │ │ │ │  API Server 1   │ │ │ │  API Server 1   │ │
│ │  │  (.NET 9)       │ │ │ │  (.NET 9)       │ │ │ │  (.NET 9)       │ │
│ │  └─────────────────┘ │ │ └─────────────────┘ │ │ └─────────────────┘ │
│ │                      │ │                      │ │                      │
│ │  ┌─────────────────┐ │ │ ┌─────────────────┐ │ │ ┌─────────────────┐ │
│ │  │  API Server 2   │ │ │ │  API Server 2   │ │ │ │  API Server 2   │ │
│ │  │  (.NET 9)       │ │ │ │  (.NET 9)       │ │ │ │  (.NET 9)       │ │
│ │  └─────────────────┘ │ │ └─────────────────┘ │ │ └─────────────────┘ │
│ │                      │ │                      │ │                      │
│ │  ┌─────────────────┐ │ │ ┌─────────────────┐ │ │ ┌─────────────────┐ │
│ │  │  API Server 3   │ │ │ │  API Server 3   │ │ │ │  API Server 3   │ │
│ │  │  (.NET 9)       │ │ │ │  (.NET 9)       │ │ │ │  (.NET 9)       │ │
│ │  └─────────────────┘ │ │ └─────────────────┘ │ │ └─────────────────┘ │
│ │                      │ │                      │ │                      │
│ └────────┬─────────────┘ │ └────────┬─────────────┘ │ └────────┬─────────────┘
│          │               │          │               │          │
│ ┌────────────────────┐   │ ┌────────────────────┐   │ ┌────────────────────┐
│ │ Cache Layer (Redis)│   │ │ Cache Layer (Redis)│   │ │ Cache Layer (Redis)│
│ │                    │   │ │                    │   │ │                    │
│ │ ┌──────────────┐   │   │ │ ┌──────────────┐   │   │ │ ┌──────────────┐   │
│ │ │ Cluster (5   │   │   │ │ │ Cluster (3   │   │   │ │ │ Cluster (3   │   │
│ │ │ nodes)       │   │   │ │ │ nodes)       │   │   │ │ │ nodes)       │   │
│ │ │              │   │   │ │ │              │   │   │ │ │              │   │
│ │ │ - User Cache │   │   │ │ │ - User Cache │   │   │ │ │ - User Cache │   │
│ │ │ - Monitor    │   │   │ │ │ - Monitor    │   │   │ │ │ - Monitor    │   │
│ │ │   Cache      │   │   │ │ │   Cache      │   │   │ │ │   Cache      │   │
│ │ │ - Rate Limit │   │   │ │ │ - Rate Limit │   │   │ │ │ - Rate Limit │   │
│ │ │ - Sessions   │   │   │ │ │ - Sessions   │   │   │ │ │ - Sessions   │   │
│ │ └──────────────┘   │   │ │ └──────────────┘   │   │ │ └──────────────┘   │
│ └────────┬───────────┘   │ └────────┬───────────┘   │ └────────┬───────────┘
│          │               │          │               │          │
│ ┌────────────────────┐   │ ┌────────────────────┐   │ ┌────────────────────┐
│ │ Database Layer     │   │ │ Database Layer     │   │ │ Database Layer     │
│ │ (PostgreSQL)       │   │ │ (PostgreSQL)       │   │ │ (PostgreSQL)       │
│ │                    │   │ │                    │   │ │                    │
│ │ ┌─────────────┐    │   │ │ ┌─────────────┐    │   │ │ ┌─────────────┐    │
│ │ │ Primary DB  │    │   │ │ │ Replica DB  │    │   │ │ │ Replica DB  │    │
│ │ │ (RW)        │    │   │ │ │ (RO)        │    │   │ │ │ (RO)        │    │
│ │ │             │    │   │ │ │             │    │   │ │ │             │    │
│ │ │ - Metrics   │    │   │ │ │ - Mirrors   │    │   │ │ │ - Mirrors   │    │
│ │ │ - Backups   │    │   │ │ │   Primary   │    │   │ │ │   Primary   │    │
│ │ │ (multi-vol) │    │   │ │ │   DB        │    │   │ │ │   DB        │    │
│ │ │             │    │   │ │ │ - Hot       │    │   │ │ │ - Hot       │    │
│ │ │             │    │   │ │ │   standby   │    │   │ │ │   standby   │    │
│ │ └─────────────┘    │   │ │ └─────────────┘    │   │ │ └─────────────┘    │
│ │                    │   │ │                    │   │ │                    │
│ │ ┌─────────────┐    │   │ │ ┌─────────────┐    │   │ │ ┌─────────────┐    │
│ │ │ Backup DB   │    │   │ │ │ Backup DB   │    │   │ │ │ Backup DB   │    │
│ │ │ (RO)        │    │   │ │ │ (RO)        │    │   │ │ │ (RO)        │    │
│ │ │ (Delayed    │    │   │ │ │ (Delayed    │    │   │ │ │ (Delayed    │    │
│ │ │  replica)   │    │   │ │ │  replica)   │    │   │ │ │  replica)   │    │
│ │ └─────────────┘    │   │ │ └─────────────┘    │   │ │ └─────────────┘    │
│ └────────┬───────────┘   │ └────────┬───────────┘   │ └────────┬───────────┘
│          │               │          │               │          │
│ ┌────────────────────┐   │ ┌────────────────────┐   │ ┌────────────────────┐
│ │ Job Queue          │   │ │ Job Queue          │   │ │ Job Queue          │
│ │ (Hangfire/RabbitMQ)│   │ │ (Hangfire/RabbitMQ)│   │ │ (Hangfire/RabbitMQ)│
│ │                    │   │ │                    │   │ │                    │
│ │ - Check Jobs       │   │ │ - Check Jobs       │   │ │ - Check Jobs       │
│ │ - Alert Jobs       │   │ │ - Alert Jobs       │   │ │ - Alert Jobs       │
│ │ - Report Jobs      │   │ │ - Report Jobs      │   │ │ - Report Jobs      │
│ │ - Cleanup Jobs     │   │ │ - Cleanup Jobs     │   │ │ - Cleanup Jobs     │
│ └────────────────────┘   │ └────────────────────┘   │ └────────────────────┘
└────────────────────────────────────────────────────────────────────────────┘
        ▲                  ▲                  ▲
        │                  │                  │
        │  Replication ←───┴───┬───────────────┘
        │  (every 1s)          │
        │                      │
        └──────────────────────┘
        
        Backup & DR (nightly to S3)
        ├─ Full backup every night
        ├─ Incremental backup hourly
        ├─ 30-day point-in-time recovery
        └─ Cross-region replication
```

### Monitoring Infrastructure

```
┌─────────────────────────────────────────────────────────────────┐
│         DISTRIBUTED MONITORING NODES (150+ globally)           │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│ Region: North America (40 nodes)                               │
│ ├─ US-East (10)      ├─ US-West (10)   ├─ Canada (8)          │
│ └─ US-Central (12)                                             │
│                                                                 │
│ Region: Europe (35 nodes)                                      │
│ ├─ UK (8)            ├─ Germany (8)    ├─ France (8)          │
│ └─ Central Europe (11)                                          │
│                                                                 │
│ Region: APAC (40 nodes)                                        │
│ ├─ Singapore (10)     ├─ Tokyo (10)     ├─ Sydney (10)        │
│ └─ India (10)                                                   │
│                                                                 │
│ Region: South America (15 nodes)                               │
│ ├─ Brazil (10)        └─ Argentina (5)                         │
│                                                                 │
│ Region: Middle East/Africa (20 nodes)                          │
│ ├─ Dubai (10)         └─ South Africa (10)                     │
│                                                                 │
│ Each Node Architecture:                                        │
│ ┌────────────────────────────────────────────────────────────┐ │
│ │ ┌─────────────────────────────────────────────────────┐  │ │
│ │ │ Monitoring Service (Go/Rust microservice)           │  │ │
│ │ │ - HTTP Checks (GET, POST, PUT, DELETE, PATCH)       │  │ │
│ │ │ - TCP/UDP Port checks                               │  │ │
│ │ │ - DNS resolution verification                       │  │ │
│ │ │ - SSL certificate validation                        │  │ │
│ │ │ - Custom synthetic checks (Selenium)                │  │ │
│ │ │ - WebSocket checks                                  │  │ │
│ │ │ - GraphQL checks                                    │  │ │
│ │ └─────────────────────────────────────────────────────┘  │ │
│ │                          ▼                                │ │
│ │ ┌─────────────────────────────────────────────────────┐  │ │
│ │ │ Results Aggregation                                 │  │ │
│ │ │ - Compress results                                  │  │ │
│ │ │ - Sign with HMAC                                    │  │ │
│ │ │ - Buffer locally (5-min window)                     │  │ │
│ │ └─────────────────────────────────────────────────────┘  │ │
│ │                          ▼                                │ │
│ │ ┌─────────────────────────────────────────────────────┐  │ │
│ │ │ Queue (Kafka/RabbitMQ)                              │  │ │
│ │ │ - Async upload to central aggregation               │  │ │
│ │ │ - Retry logic with exponential backoff              │  │ │
│ │ │ - Disk-based fallback (20GB buffer)                 │  │ │
│ │ └─────────────────────────────────────────────────────┘  │ │
│ │                          ▼                                │ │
│ │ ┌─────────────────────────────────────────────────────┐  │ │
│ │ │ Upload to Central API                               │  │ │
│ │ │ - Every 5 seconds                                   │  │ │
│ │ │ - Compressed batch format                           │  │ │
│ │ │ - Automatic failover to secondary region            │  │ │
│ │ └─────────────────────────────────────────────────────┘  │ │
│ └────────────────────────────────────────────────────────────┘ │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                             ▼
                   ┌────────────────────┐
                   │ Central Aggregation │
                   │ (ECS/EKS Service)   │
                   │                    │
                   │ - Ingest results   │
                   │ - De-duplicate     │
                   │ - Detect anomalies │
                   │ - Trigger alerts   │
                   │ - Store in DB      │
                   └────────────────────┘
```

### Security & Network Architecture

```
┌──────────────────────────────────────────────────────────┐
│                 SECURITY LAYERS                          │
├──────────────────────────────────────────────────────────┤
│                                                          │
│ Layer 1: DDoS Protection & WAF                          │
│ ┌────────────────────────────────────────────────────┐ │
│ │ AWS Shield (DDoS) + CloudFlare WAF                 │ │
│ │ - Automatic DDoS mitigation                        │ │
│ │ - OWASP Top 10 protection                          │ │
│ │ - Rate limiting (1000 req/min per IP)              │ │
│ │ - Bot management                                   │ │
│ └────────────────────────────────────────────────────┘ │
│                          ▼                              │
│ Layer 2: API Gateway & Authentication                  │
│ ┌────────────────────────────────────────────────────┐ │
│ │ AWS API Gateway + Kong (optional)                  │ │
│ │ - TLS 1.3 enforcement                              │ │
│ │ - JWT token validation                             │ │
│ │ - API key verification                             │ │
│ │ - Request signing (HMAC-SHA256)                    │ │
│ └────────────────────────────────────────────────────┘ │
│                          ▼                              │
│ Layer 3: Application Security                          │
│ ┌────────────────────────────────────────────────────┐ │
│ │ .NET 9 Security Features                           │ │
│ │ - Input validation (FluentValidation)              │ │
│ │ - CORS (cross-origin whitelist)                    │ │
│ │ - CSRF tokens                                      │ │
│ │ - SQL injection prevention (ORM)                   │ │
│ │ - XSS prevention (Content Security Policy)         │ │
│ │ - Secrets management (AWS Secrets Manager)         │ │
│ └────────────────────────────────────────────────────┘ │
│                          ▼                              │
│ Layer 4: Database Security                             │
│ ┌────────────────────────────────────────────────────┐ │
│ │ PostgreSQL Hardening                               │ │
│ │ - Encryption at rest (AWS RDS encryption)          │ │
│ │ - Encryption in transit (SSL)                      │ │
│ │ - Row-level security (RLS)                         │ │
│ │ - Column encryption (for sensitive fields)         │ │
│ │ - Audit logging (all queries)                      │ │
│ │ - Private subnet (no public IP)                    │ │
│ └────────────────────────────────────────────────────┘ │
│                          ▼                              │
│ Layer 5: Infrastructure Security                       │
│ ┌────────────────────────────────────────────────────┐ │
│ │ Network & Compliance                               │ │
│ │ - VPC with private subnets                         │ │
│ │ - Security groups (least privilege)                │ │
│ │ - NACLs for additional filtering                   │ │
│ │ - VPN access for admin functions                   │ │
│ │ - Multi-factor authentication (2FA required)       │ │
│ │ - Audit logging (CloudTrail)                       │ │
│ └────────────────────────────────────────────────────┘ │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

---

## 2. API Standards

### 2.1 API Specification

**Format**: RESTful with JSON payload
**Authentication**: JWT (Bearer token) + API Keys
**API Version**: v1 (in URL path)
**Base URL**: `https://api.monitoring.com/v1`

### 2.2 Request/Response Format

```json
// Standard Request
{
  "data": { 
    // Request body specific to endpoint
  },
  "metadata": {
    "correlation_id": "uuid-string",
    "request_timestamp": "2026-05-30T10:30:00Z"
  }
}

// Standard Response (Success)
{
  "data": {
    // Response data
  },
  "meta": {
    "correlation_id": "uuid-string",
    "timestamp": "2026-05-30T10:30:00Z",
    "request_id": "unique-request-id"
  },
  "pagination": {
    "page": 1,
    "page_size": 20,
    "total_count": 150,
    "has_more": true
  }
}

// Standard Response (Error)
{
  "error": {
    "code": "INVALID_REQUEST",
    "message": "Human-readable error message",
    "details": {
      "field": "email",
      "issue": "Invalid email format"
    },
    "correlation_id": "uuid-string"
  },
  "meta": {
    "timestamp": "2026-05-30T10:30:00Z",
    "request_id": "unique-request-id"
  }
}
```

### 2.3 HTTP Status Codes

| Code | Meaning | Usage |
|------|---------|-------|
| 200 | OK | Successful GET, PUT, PATCH |
| 201 | Created | Successful POST creating resource |
| 202 | Accepted | Async operation accepted |
| 204 | No Content | Successful DELETE |
| 400 | Bad Request | Invalid input, validation error |
| 401 | Unauthorized | Missing/invalid authentication |
| 403 | Forbidden | Authenticated but no permission |
| 404 | Not Found | Resource doesn't exist |
| 409 | Conflict | Resource state conflict (duplicate) |
| 422 | Unprocessable Entity | Validation error in request body |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Internal Server Error | Unexpected server error |
| 503 | Service Unavailable | Maintenance or temporary outage |

### 2.4 Rate Limiting

```
Headers:
- X-RateLimit-Limit: 1000 (requests per hour)
- X-RateLimit-Remaining: 999 (requests remaining)
- X-RateLimit-Reset: 1685334600 (unix timestamp)

Rules:
- Starter plan: 1,000 req/hour
- Professional: 10,000 req/hour
- Enterprise: Unlimited (default 100,000)

Burst allowance: 10x per-second peak
Retry-After header: Returned on 429 response
```

### 2.5 API Endpoints Structure

```
# Monitors
GET    /organizations/{orgId}/monitors              # List
POST   /organizations/{orgId}/monitors              # Create
GET    /organizations/{orgId}/monitors/{monitorId}  # Get
PUT    /organizations/{orgId}/monitors/{monitorId}  # Update
DELETE /organizations/{orgId}/monitors/{monitorId}  # Delete
GET    /organizations/{orgId}/monitors/{monitorId}/uptime  # Uptime

# Check Results
GET    /organizations/{orgId}/monitors/{monitorId}/checks  # History

# Incidents
GET    /organizations/{orgId}/incidents             # List
GET    /organizations/{orgId}/incidents/{incidentId}# Get
PUT    /organizations/{orgId}/incidents/{incidentId}# Update

# Alert Channels
GET    /organizations/{orgId}/alert-channels        # List
POST   /organizations/{orgId}/alert-channels        # Create
PUT    /organizations/{orgId}/alert-channels/{channelId}  # Update
DELETE /organizations/{orgId}/alert-channels/{channelId}  # Delete

# Alert Policies
GET    /organizations/{orgId}/monitors/{monitorId}/alert-policies  # List
POST   /organizations/{orgId}/monitors/{monitorId}/alert-policies  # Create
PUT    /organizations/{orgId}/monitors/{monitorId}/alert-policies/{policyId}  # Update
DELETE /organizations/{orgId}/monitors/{monitorId}/alert-policies/{policyId}  # Delete

# Analytics
GET    /organizations/{orgId}/analytics/uptime      # Uptime trends
GET    /organizations/{orgId}/analytics/performance # Performance trends
GET    /organizations/{orgId}/analytics/incidents   # Incident analytics

# Users (Team Management)
GET    /organizations/{orgId}/users                 # List
POST   /organizations/{orgId}/users                 # Invite
PUT    /organizations/{orgId}/users/{userId}        # Update
DELETE /organizations/{orgId}/users/{userId}        # Remove

# Billing
GET    /organizations/{orgId}/billing               # Get billing info
POST   /organizations/{orgId}/billing/upgrade       # Upgrade plan
GET    /organizations/{orgId}/invoices              # Get invoices

# Status Pages
GET    /organizations/{orgId}/status-pages          # List
POST   /organizations/{orgId}/status-pages          # Create
PUT    /organizations/{orgId}/status-pages/{pageId}  # Update
DELETE /organizations/{orgId}/status-pages/{pageId}  # Delete

# Public Status Page (No auth required)
GET    /status/{pageSlug}                           # Get public page
GET    /status/{pageSlug}/incidents                 # Get incidents
GET    /status/{pageSlug}/subscribe                 # Subscribe endpoint
```

### 2.6 API Documentation

**Tool**: Swagger/OpenAPI 3.0
**Format**: YAML
**Hosting**: `/swagger` and `/swagger-ui`
**Authentication**: Documented with examples
**Rate Limits**: Documented per endpoint
**Response Examples**: All endpoints have example requests/responses

---

## 3. Coding Standards

### 3.1 C# / .NET Coding Standards

#### Naming Conventions
```csharp
// Classes (PascalCase)
public class MonitorService { }

// Methods (PascalCase)
public async Task<MonitorDto> GetMonitorAsync(Guid monitorId) { }

// Properties (PascalCase)
public string MonitorName { get; set; }

// Private fields (camelCase with underscore prefix)
private readonly ILogger<MonitorService> _logger;
private string _internalState;

// Constants (UPPER_SNAKE_CASE)
private const int DEFAULT_CHECK_FREQUENCY = 300;

// Parameters (camelCase)
public void CreateMonitor(string monitorName, int checkFrequency) { }

// Local variables (camelCase)
var totalMonitors = monitors.Count();
```

#### File Organization
```
MonitorService.cs
├─ Usings (organized: System, Third-party, Internal)
├─ Namespace declaration
├─ Class declaration
│  ├─ Private fields
│  ├─ Public properties
│  ├─ Constructor
│  ├─ Public methods
│  │  ├─ Query methods
│  │  └─ Command methods
│  └─ Private helper methods
```

#### Code Structure Example
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MonitoringPlatform.Domain;
using MonitoringPlatform.Application.Interfaces;

namespace MonitoringPlatform.Application.Services;

public class MonitorService : IMonitorService
{
    private readonly IMonitorRepository _monitorRepository;
    private readonly ILogger<MonitorService> _logger;
    private readonly IIncidentService _incidentService;

    public MonitorService(
        IMonitorRepository monitorRepository,
        ILogger<MonitorService> logger,
        IIncidentService incidentService)
    {
        _monitorRepository = monitorRepository ?? throw new ArgumentNullException(nameof(monitorRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _incidentService = incidentService ?? throw new ArgumentNullException(nameof(incidentService));
    }

    public async Task<MonitorDto> GetMonitorAsync(Guid monitorId)
    {
        _logger.LogInformation("Fetching monitor {MonitorId}", monitorId);
        
        var monitor = await _monitorRepository.GetByIdAsync(monitorId);
        if (monitor == null)
        {
            _logger.LogWarning("Monitor {MonitorId} not found", monitorId);
            throw new NotFoundException($"Monitor {monitorId} not found");
        }

        return MapToDto(monitor);
    }

    public async Task CreateMonitorAsync(CreateMonitorCommand command)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ValidationException("Monitor name is required");

        // Business logic
        var monitor = Monitor.Create(command.Name, command.Url, command.Type);
        
        // Persistence
        await _monitorRepository.AddAsync(monitor);
        await _monitorRepository.SaveChangesAsync();

        _logger.LogInformation("Monitor {MonitorId} created", monitor.Id);
    }

    private MonitorDto MapToDto(Monitor monitor) => new()
    {
        Id = monitor.Id,
        Name = monitor.Name,
        Url = monitor.Url,
        Type = monitor.Type.ToString()
    };
}
```

#### CQRS Pattern Implementation
```csharp
// Command (Mutating operation)
public record CreateMonitorCommand(
    Guid OrganizationId,
    string Name,
    string Url,
    MonitorType Type,
    int CheckFrequencySeconds
) : IRequest<MonitorDto>;

// Handler
public class CreateMonitorCommandHandler : IRequestHandler<CreateMonitorCommand, MonitorDto>
{
    public async Task<MonitorDto> Handle(CreateMonitorCommand request, CancellationToken cancellationToken)
    {
        // Validation
        // Creation
        // Persistence
        // Event publishing
        return monitorDto;
    }
}

// Query (Read-only operation)
public record GetMonitorsQuery(Guid OrganizationId, int Page = 1, int PageSize = 20) 
    : IRequest<PaginatedResult<MonitorDto>>;

// Handler
public class GetMonitorsQueryHandler : IRequestHandler<GetMonitorsQuery, PaginatedResult<MonitorDto>>
{
    public async Task<PaginatedResult<MonitorDto>> Handle(GetMonitorsQuery request, CancellationToken cancellationToken)
    {
        // Fetch from cache if available
        // Otherwise fetch from database
        // Return paginated results
        return result;
    }
}
```

#### Error Handling
```csharp
// Custom exceptions (Domain layer)
public class DomainException : Exception { }
public class NotFoundException : DomainException { }
public class ValidationException : DomainException { }
public class DuplicateException : DomainException { }

// Usage
try
{
    await _monitorService.CreateMonitorAsync(command);
}
catch (ValidationException ex)
{
    _logger.LogWarning("Validation failed: {Message}", ex.Message);
    // Handle gracefully
}
catch (DomainException ex)
{
    _logger.LogError(ex, "Domain error occurred");
    // Handle gracefully
}
```

#### Testing Standards
```csharp
// Unit Test Structure
public class MonitorServiceTests
{
    private readonly IMonitorRepository _monitorRepository;
    private readonly MonitorService _service;

    public MonitorServiceTests()
    {
        _monitorRepository = Substitute.For<IMonitorRepository>();
        _service = new MonitorService(_monitorRepository, Substitute.For<ILogger<MonitorService>>());
    }

    [Fact]
    public async Task GetMonitor_WithValidId_ReturnsMonitor()
    {
        // Arrange
        var monitorId = Guid.NewGuid();
        var monitor = Monitor.Create("Test", "https://example.com", MonitorType.Http);
        _monitorRepository.GetByIdAsync(monitorId).Returns(monitor);

        // Act
        var result = await _service.GetMonitorAsync(monitorId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetMonitor_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var monitorId = Guid.NewGuid();
        _monitorRepository.GetByIdAsync(monitorId).Returns((Monitor)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetMonitorAsync(monitorId));
    }
}
```

### 3.2 Angular / TypeScript Coding Standards

#### Naming Conventions
```typescript
// Classes (PascalCase)
export class MonitorService { }

// Interfaces (PascalCase with 'I' prefix - optional)
export interface IMonitor { }
export interface Monitor { }

// Enums (PascalCase)
export enum MonitorStatus { Active, Paused, Deleted }

// Types (PascalCase)
export type MonitorType = 'http' | 'tcp' | 'dns';

// Functions (camelCase)
export function calculateUptime(checks: CheckResult[]): number { }

// Constants (UPPER_SNAKE_CASE)
export const DEFAULT_CHECK_FREQUENCY = 300;

// Variables (camelCase)
let totalMonitors = 0;
const monitorList: Monitor[] = [];

// Private members (_prefix)
private _monitorCache = new Map<string, Monitor>();
```

#### Component Structure
```typescript
// monitor-list.component.ts
import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { MonitorService } from '@services/monitor.service';
import { Monitor } from '@models/monitor.model';

@Component({
  selector: 'app-monitor-list',
  templateUrl: './monitor-list.component.html',
  styleUrls: ['./monitor-list.component.scss']
})
export class MonitorListComponent implements OnInit, OnDestroy {
  // Signals (Angular 20)
  monitors = signal<Monitor[]>([]);
  isLoading = signal(false);
  selectedMonitor = signal<Monitor | null>(null);

  // Legacy observables (with proper cleanup)
  private destroy$ = new Subject<void>();

  // Form
  filterForm: FormGroup;

  // View state
  displayColumns = ['name', 'url', 'status', 'uptime', 'actions'];

  constructor(
    private monitorService: MonitorService,
    private formBuilder: FormBuilder
  ) {
    this.filterForm = this.formBuilder.group({
      search: [''],
      status: ['all'],
      sortBy: ['name']
    });
  }

  ngOnInit(): void {
    this.loadMonitors();
    this.setupFiltering();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadMonitors(): void {
    this.isLoading.set(true);
    
    this.monitorService
      .getMonitors()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (monitors) => {
          this.monitors.set(monitors);
          this.isLoading.set(false);
        },
        error: (error) => {
          console.error('Error loading monitors:', error);
          this.isLoading.set(false);
        }
      });
  }

  private setupFiltering(): void {
    this.filterForm.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe((filters) => {
        this.loadMonitors();
      });
  }

  selectMonitor(monitor: Monitor): void {
    this.selectedMonitor.set(monitor);
  }

  deleteMonitor(monitorId: string): void {
    this.monitorService
      .deleteMonitor(monitorId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.loadMonitors();
      });
  }
}
```

#### Service Structure
```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';

import { Monitor } from '@models/monitor.model';
import { ApiResponse } from '@models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class MonitorService {
  private apiUrl = 'api/v1/organizations/current/monitors';
  private monitorCache$ = new BehaviorSubject<Monitor[]>([]);

  constructor(private http: HttpClient) { }

  getMonitors(): Observable<Monitor[]> {
    return this.http.get<ApiResponse<Monitor[]>>(this.apiUrl)
      .pipe(
        map(response => response.data),
        tap(monitors => this.monitorCache$.next(monitors)),
        catchError(error => {
          console.error('Failed to fetch monitors:', error);
          return of([]);
        })
      );
  }

  getMonitor(id: string): Observable<Monitor> {
    return this.http.get<ApiResponse<Monitor>>(`${this.apiUrl}/${id}`)
      .pipe(
        map(response => response.data),
        catchError(error => {
          console.error(`Failed to fetch monitor ${id}:`, error);
          throw error;
        })
      );
  }

  createMonitor(monitor: Partial<Monitor>): Observable<Monitor> {
    return this.http.post<ApiResponse<Monitor>>(this.apiUrl, { data: monitor })
      .pipe(
        map(response => response.data),
        tap(newMonitor => {
          const current = this.monitorCache$.value;
          this.monitorCache$.next([...current, newMonitor]);
        })
      );
  }

  updateMonitor(id: string, monitor: Partial<Monitor>): Observable<Monitor> {
    return this.http.put<ApiResponse<Monitor>>(`${this.apiUrl}/${id}`, { data: monitor })
      .pipe(
        map(response => response.data),
        tap(updated => {
          const current = this.monitorCache$.value;
          const index = current.findIndex(m => m.id === id);
          if (index >= 0) {
            current[index] = updated;
            this.monitorCache$.next([...current]);
          }
        })
      );
  }

  deleteMonitor(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`)
      .pipe(
        tap(() => {
          const current = this.monitorCache$.value;
          this.monitorCache$.next(current.filter(m => m.id !== id));
        })
      );
  }
}
```

#### Angular Signals (Angular 20)
```typescript
// Use computed signals for derived state
export class DashboardComponent {
  monitors = signal<Monitor[]>([]);
  
  activeMonitorsCount = computed(() => 
    this.monitors().filter(m => m.status === 'active').length
  );
  
  averageUptime = computed(() => {
    const monitors = this.monitors();
    if (monitors.length === 0) return 0;
    const total = monitors.reduce((sum, m) => sum + m.uptime, 0);
    return total / monitors.length;
  });
}
```

### 3.3 Styling Standards (SCSS)

```scss
// variables.scss
$primary-color: #0066cc;
$secondary-color: #666;
$danger-color: #cc0000;
$warning-color: #ff9900;
$success-color: #00cc66;

$spacing-unit: 1rem; // 16px
$border-radius: 4px;

// component.component.scss
.monitor-card {
  display: flex;
  gap: $spacing-unit;
  padding: $spacing-unit * 1.5;
  background: white;
  border-radius: $border-radius;
  border: 1px solid #ddd;

  &__header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: $spacing-unit;
  }

  &__title {
    font-size: 1.25rem;
    font-weight: 600;
    color: $secondary-color;
  }

  &__status {
    padding: 0.25rem 0.75rem;
    border-radius: $border-radius;
    font-size: 0.875rem;
    font-weight: 500;

    &--active {
      background: #e6f3ff;
      color: $primary-color;
    }

    &--paused {
      background: #ffe6e6;
      color: $warning-color;
    }
  }
}
```

---

## Conclusion

These standards ensure:
- **Consistency**: Uniform code style across backend and frontend
- **Maintainability**: Easy to understand and modify code
- **Performance**: Optimized patterns and practices
- **Security**: Built-in security considerations
- **Scalability**: Code structure supports growth
