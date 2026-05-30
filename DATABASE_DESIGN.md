# Database Design & Schema

## 1. Database Entity Relationship Diagram (ERD)

### High-Level Relationships
```
Organization (1) ────────────(N) Team
                └──────────────(N) User
                └──────────────(N) Billing

Team (1) ─────────────────────(N) User
Team (1) ─────────────────────(N) Monitor

User (1) ───────────────────(N) Monitor (created_by)
User (1) ───────────────────(N) AlertChannel

Monitor (1) ────────────────(N) CheckResult
Monitor (1) ────────────────(N) Incident
Monitor (1) ────────────────(N) AlertPolicy

AlertPolicy (1) ──────────(N) AlertChannel
AlertChannel (1) ───────(N) AlertLog

Incident (1) ─────────────(N) IncidentEvent
Incident (1) ─────────────(N) IncidentComment

Webhook (1) ────────────(N) WebhookLog
```

### Detailed ERD
```
┌─────────────────────────────────────────────────────────────┐
│                   ORGANIZATIONS                             │
├─────────────────────────────────────────────────────────────┤
│ ◉ OrganizationId (PK, UUID)                                 │
│  • Name (VARCHAR 255)                                       │
│  • Slug (VARCHAR 100, UNIQUE)                               │
│  • Status (ENUM: Active, Suspended, Deleted)                │
│  • PlanType (ENUM: Starter, Professional, Enterprise)       │
│  • MaxMonitors (INT)                                        │
│  • MaxAlerts (INT)                                          │
│  • BillingEmail (VARCHAR 255)                               │
│  • TimeZone (VARCHAR 100)                                   │
│  • CustomDomain (VARCHAR 255)                               │
│  • LogoUrl (TEXT)                                           │
│  • CreatedAt (TIMESTAMP)                                    │
│  • UpdatedAt (TIMESTAMP)                                    │
│  • DeletedAt (TIMESTAMP, nullable)                          │
└─────────────────────────────────────────────────────────────┘
              ▲                                    ▲
              │                                    │
        (1:N) │                              (1:N) │
              │                                    │
┌─────────────┴─────────────┐    ┌────────────────┴──────────┐
│         TEAMS             │    │        USERS              │
├───────────────────────────┤    ├──────────────────────────┤
│ ◉ TeamId (PK, UUID)       │    │ ◉ UserId (PK, UUID)      │
│ ◉ OrganizationId (FK)     │    │ ◉ OrganizationId (FK)    │
│  • Name (VARCHAR 255)     │    │ ◉ TeamId (FK, nullable)  │
│  • Description (TEXT)     │    │  • Email (VARCHAR 255)   │
│  • CreatedAt (TIMESTAMP)  │    │  • FirstName (VARCHAR)   │
│  • UpdatedAt (TIMESTAMP)  │    │  • LastName (VARCHAR)    │
└───────────────────────────┘    │  • PasswordHash (TEXT)   │
                                  │  • Role (ENUM: Owner,    │
                                  │    Admin, Member, RO)    │
                                  │  • Status (ENUM: Active, │
                                  │    Inactive, Suspended)  │
                                  │  • TwoFactorEnabled      │
                                  │  • LastLoginAt           │
                                  │  • CreatedAt             │
                                  │  • UpdatedAt             │
                                  └──────────────────────────┘
                                           ▲
                                    (1:N)  │
                                           │
                ┌──────────────────────────┴─────────────────┐
                │         MONITORS                          │
                ├──────────────────────────────────────────┤
                │ ◉ MonitorId (PK, UUID)                   │
                │ ◉ OrganizationId (FK)                    │
                │ ◉ TeamId (FK, nullable)                  │
                │ ◉ CreatedByUserId (FK)                   │
                │  • Name (VARCHAR 255)                    │
                │  • Type (ENUM: HTTP, TCP, DNS, SSL, API) │
                │  • Url (TEXT)                            │
                │  • Description (TEXT)                    │
                │  • Status (ENUM: Active, Paused, Deleted)│
                │  • CheckFrequency (INT seconds, def: 300)│
                │  • Timeout (INT seconds, def: 30)        │
                │  • HttpMethod (VARCHAR, for HTTP)        │
                │  • ExpectedStatusCode (INT)              │
                │  • ResponseBodyMatch (TEXT, nullable)    │
                │  • HeadersJson (JSONB)                   │
                │  • AuthUsername (VARCHAR, encrypted)     │
                │  • AuthPassword (VARCHAR, encrypted)     │
                │  • NotifyChannels (UUID[], JSON)         │
                │  • Tags (VARCHAR[], JSON)                │
                │  • CreatedAt (TIMESTAMP)                 │
                │  • UpdatedAt (TIMESTAMP)                 │
                │  • DeletedAt (TIMESTAMP, nullable)       │
                └──────────────────────────────────────────┘
                         ▲              ▲
                   (1:N) │              │ (1:N)
                         │              │
        ┌────────────────┴──────┐    ┌──┴───────────────────┐
        │   CHECK_RESULTS       │    │   INCIDENTS          │
        ├───────────────────────┤    ├──────────────────────┤
        │ ◉ CheckResultId (PK)  │    │ ◉ IncidentId (PK)    │
        │ ◉ MonitorId (FK)      │    │ ◉ MonitorId (FK)     │
        │  • Timestamp          │    │  • Status (ENUM)     │
        │  • ResponseCode       │    │  • StartTime         │
        │  • ResponseTime (ms)  │    │  • EndTime (nullable)│
        │  • IsSuccess (BOOL)   │    │  • Duration          │
        │  • ErrorMessage       │    │  • Impact (INT)      │
        │  • Region (VARCHAR)   │    │  • RootCause (TEXT) │
        │  • CheckDuration      │    │  • CreatedAt         │
        │  • CreatedAt          │    │  • UpdatedAt         │
        └───────────────────────┘    │  • ResolvedAt        │
                                     │  • ResolvedBy (FK)   │
                                     └──────────────────────┘
                                              ▲
                                        (1:N) │
                                             │
                                    ┌────────┴──────────┐
                                    │ INCIDENT_EVENTS   │
                                    ├───────────────────┤
                                    │ ◉ EventId (PK)    │
                                    │ ◉ IncidentId (FK) │
                                    │  • EventType      │
                                    │  • Message        │
                                    │  • CreatedBy      │
                                    │  • CreatedAt      │
                                    └───────────────────┘

┌─────────────────────────────────────────────────────────────┐
│           ALERT_CHANNELS (Notification Targets)            │
├─────────────────────────────────────────────────────────────┤
│ ◉ AlertChannelId (PK, UUID)                                 │
│ ◉ OrganizationId (FK)                                       │
│ ◉ UserId (FK, nullable - for personal channels)             │
│  • Type (ENUM: Email, Slack, Teams, PagerDuty, Webhook)    │
│  • Name (VARCHAR 255)                                      │
│  • Configuration (JSONB - encrypted credentials)            │
│  • IsActive (BOOL)                                         │
│  • VerificationStatus (ENUM: Pending, Verified, Failed)     │
│  • CreatedAt (TIMESTAMP)                                   │
│  • UpdatedAt (TIMESTAMP)                                   │
└─────────────────────────────────────────────────────────────┘
              ▲                            ▲
        (1:N) │                      (1:N) │
              │                            │
       ┌──────┴─────────┐    ┌────────────┴──────────┐
       │ ALERT_POLICIES │    │  ALERT_LOGS           │
       ├────────────────┤    ├───────────────────────┤
       │ ◉ PolicyId     │    │ ◉ LogId               │
       │ ◉ MonitorId    │    │ ◉ ChannelId (FK)      │
       │ ◉ ChannelId    │    │ ◉ IncidentId (FK,n.) │
       │  • Condition   │    │  • MessageType        │
       │  • Threshold   │    │  • Status             │
       │  • Enabled     │    │  • ResponseStatus     │
       │  • CreatedAt   │    │  • ErrorMsg (n.)      │
       └────────────────┘    │  • SentAt             │
                             │  • CreatedAt          │
                             └───────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│              STATUS_PAGES (Public Pages)                    │
├─────────────────────────────────────────────────────────────┤
│ ◉ PageId (PK, UUID)                                         │
│ ◉ OrganizationId (FK)                                       │
│  • Name (VARCHAR 255)                                      │
│  • Slug (VARCHAR 100, UNIQUE per org)                      │
│  • Description (TEXT)                                      │
│  • Visibility (ENUM: Public, Private, Logged-In)            │
│  • CustomDomain (VARCHAR 255)                              │
│  • MonitorIds (UUID[], JSON - which monitors to show)       │
│  • CreatedAt (TIMESTAMP)                                   │
│  • UpdatedAt (TIMESTAMP)                                   │
└─────────────────────────────────────────────────────────────┘
                    ▲
              (1:N) │
                    │
         ┌──────────┴──────────┐
         │ STATUS_PAGE_UPDATES │
         ├─────────────────────┤
         │ ◉ UpdateId (PK)     │
         │ ◉ PageId (FK)       │
         │  • Title            │
         │  • Description      │
         │  • Status (ENUM)    │
         │  • CreatedAt        │
         │  • UpdatedAt        │
         │  • ResolvedAt (n.)  │
         └─────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                 WEBHOOKS (Integration)                      │
├─────────────────────────────────────────────────────────────┤
│ ◉ WebhookId (PK, UUID)                                      │
│ ◉ OrganizationId (FK)                                       │
│  • Url (TEXT)                                              │
│  • Events (VARCHAR[], JSON - which events trigger)          │
│  • IsActive (BOOL)                                         │
│  • Secret (TEXT, encrypted - for HMAC signing)              │
│  • CreatedAt (TIMESTAMP)                                   │
│  • UpdatedAt (TIMESTAMP)                                   │
└─────────────────────────────────────────────────────────────┘
              ▲
        (1:N) │
              │
       ┌──────┴──────────┐
       │ WEBHOOK_LOGS    │
       ├─────────────────┤
       │ ◉ LogId (PK)    │
       │ ◉ WebhookId     │
       │  • EventType    │
       │  • Payload      │
       │  • Response     │
       │  • StatusCode   │
       │  • Success      │
       │  • CreatedAt    │
       │  • RetriesLeft  │
       └─────────────────┘

┌─────────────────────────────────────────────────────────────┐
│              AUDIT_LOGS (Compliance)                        │
├─────────────────────────────────────────────────────────────┤
│ ◉ LogId (PK, BIGINT - for performance)                      │
│ ◉ OrganizationId (FK)                                       │
│ ◉ UserId (FK, nullable - for system actions)                │
│  • Action (VARCHAR 255)                                    │
│  • ResourceType (VARCHAR 100)                              │
│  • ResourceId (UUID, nullable)                             │
│  • Changes (JSONB - before/after state)                    │
│  • IpAddress (VARCHAR)                                     │
│  • UserAgent (TEXT)                                        │
│  • CreatedAt (TIMESTAMP, indexed)                          │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                BILLING (Subscription)                       │
├─────────────────────────────────────────────────────────────┤
│ ◉ BillingId (PK, UUID)                                      │
│ ◉ OrganizationId (FK, UNIQUE)                               │
│  • Plan (ENUM: Starter, Professional, Enterprise)          │
│  • BillingCycle (ENUM: Monthly, Annual)                    │
│  • Amount (DECIMAL)                                        │
│  • Currency (VARCHAR 3)                                    │
│  • StripeCustomerId (VARCHAR, encrypted)                   │
│  • StripeSubscriptionId (VARCHAR, encrypted)                │
│  • Status (ENUM: Active, Cancelled, Overdue)                │
│  • NextBillingDate (TIMESTAMP)                             │
│  • CancelledAt (TIMESTAMP, nullable)                       │
│  • CreatedAt (TIMESTAMP)                                   │
│  • UpdatedAt (TIMESTAMP)                                   │
└─────────────────────────────────────────────────────────────┘
              ▲
        (1:N) │
              │
       ┌──────┴──────────┐
       │ INVOICES        │
       ├─────────────────┤
       │ ◉ InvoiceId     │
       │ ◉ BillingId     │
       │  • Amount       │
       │  • PeriodStart  │
       │  • PeriodEnd    │
       │  • Status       │
       │  • PaidAt (n.)  │
       │  • CreatedAt    │
       └─────────────────┘
```

---

## 2. Complete PostgreSQL Schema

### Core Tables

```sql
-- ============================================================================
-- ORGANIZATIONS
-- ============================================================================
CREATE TABLE organizations (
    organization_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    slug VARCHAR(100) NOT NULL UNIQUE,
    status VARCHAR(50) NOT NULL DEFAULT 'active' 
        CHECK (status IN ('active', 'suspended', 'deleted')),
    plan_type VARCHAR(50) NOT NULL DEFAULT 'starter'
        CHECK (plan_type IN ('starter', 'professional', 'enterprise')),
    max_monitors INTEGER NOT NULL DEFAULT 10,
    max_alerts INTEGER NOT NULL DEFAULT 3,
    billing_email VARCHAR(255),
    time_zone VARCHAR(100) DEFAULT 'UTC',
    custom_domain VARCHAR(255),
    logo_url TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT valid_org_status CHECK (status IN ('active', 'suspended', 'deleted'))
);

CREATE INDEX idx_organizations_slug ON organizations(slug);
CREATE INDEX idx_organizations_status ON organizations(status);
CREATE INDEX idx_organizations_created_at ON organizations(created_at);

-- ============================================================================
-- TEAMS
-- ============================================================================
CREATE TABLE teams (
    team_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    organization_id UUID NOT NULL REFERENCES organizations(organization_id),
    name VARCHAR(255) NOT NULL,
    description TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    UNIQUE(organization_id, name)
);

CREATE INDEX idx_teams_organization_id ON teams(organization_id);

-- ============================================================================
-- USERS
-- ============================================================================
CREATE TABLE users (
    user_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    organization_id UUID NOT NULL REFERENCES organizations(organization_id),
    team_id UUID REFERENCES teams(team_id),
    email VARCHAR(255) NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    password_hash TEXT NOT NULL,
    role VARCHAR(50) NOT NULL DEFAULT 'member'
        CHECK (role IN ('owner', 'admin', 'member', 'read_only')),
    status VARCHAR(50) NOT NULL DEFAULT 'active'
        CHECK (status IN ('active', 'inactive', 'suspended')),
    two_factor_enabled BOOLEAN DEFAULT FALSE,
    two_factor_secret VARCHAR(255),
    last_login_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    UNIQUE(organization_id, email)
);

CREATE INDEX idx_users_organization_id ON users(organization_id);
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_status ON users(status);
CREATE INDEX idx_users_role ON users(role);

-- ============================================================================
-- MONITORS
-- ============================================================================
CREATE TABLE monitors (
    monitor_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    organization_id UUID NOT NULL REFERENCES organizations(organization_id),
    team_id UUID REFERENCES teams(team_id),
    created_by_user_id UUID NOT NULL REFERENCES users(user_id),
    name VARCHAR(255) NOT NULL,
    type VARCHAR(50) NOT NULL
        CHECK (type IN ('http', 'tcp', 'dns', 'ssl', 'api')),
    url TEXT NOT NULL,
    description TEXT,
    status VARCHAR(50) NOT NULL DEFAULT 'active'
        CHECK (status IN ('active', 'paused', 'deleted')),
    check_frequency_seconds INTEGER NOT NULL DEFAULT 300,
    timeout_seconds INTEGER NOT NULL DEFAULT 30,
    
    -- HTTP-specific
    http_method VARCHAR(10) DEFAULT 'GET',
    expected_status_code INTEGER,
    response_body_match TEXT,
    
    -- Configuration (JSON)
    headers_json JSONB,
    auth_username VARCHAR(255),
    auth_password VARCHAR(255), -- Encrypted in application
    
    -- Metadata
    tags JSONB DEFAULT '[]'::jsonb,
    notify_channels JSONB DEFAULT '[]'::jsonb,
    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT valid_monitor_type CHECK (type IN ('http', 'tcp', 'dns', 'ssl', 'api'))
);

CREATE INDEX idx_monitors_organization_id ON monitors(organization_id);
CREATE INDEX idx_monitors_team_id ON monitors(team_id);
CREATE INDEX idx_monitors_status ON monitors(status);
CREATE INDEX idx_monitors_created_at ON monitors(created_at);
CREATE INDEX idx_monitors_created_by ON monitors(created_by_user_id);

-- ============================================================================
-- CHECK_RESULTS
-- ============================================================================
CREATE TABLE check_results (
    check_result_id BIGSERIAL PRIMARY KEY,
    monitor_id UUID NOT NULL REFERENCES monitors(monitor_id) ON DELETE CASCADE,
    timestamp TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    response_code INTEGER,
    response_time_ms INTEGER,
    is_success BOOLEAN NOT NULL,
    error_message TEXT,
    region VARCHAR(100),
    check_duration_ms INTEGER,
    
    -- Extended attributes (JSON for flexibility)
    metadata JSONB
);

-- Partitioning for performance (partition by month)
-- CREATE TABLE check_results_2026_05 PARTITION OF check_results
--     FOR VALUES FROM ('2026-05-01') TO ('2026-06-01');

CREATE INDEX idx_check_results_monitor_id ON check_results(monitor_id);
CREATE INDEX idx_check_results_timestamp ON check_results(timestamp DESC);
CREATE INDEX idx_check_results_monitor_timestamp ON check_results(monitor_id, timestamp DESC);

-- ============================================================================
-- INCIDENTS
-- ============================================================================
CREATE TABLE incidents (
    incident_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    monitor_id UUID NOT NULL REFERENCES monitors(monitor_id),
    status VARCHAR(50) NOT NULL DEFAULT 'ongoing'
        CHECK (status IN ('ongoing', 'investigating', 'monitoring', 'resolved')),
    start_time TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    end_time TIMESTAMP WITH TIME ZONE,
    duration_seconds INTEGER,
    impact_count INTEGER NOT NULL DEFAULT 1,
    root_cause TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    resolved_at TIMESTAMP WITH TIME ZONE,
    resolved_by_user_id UUID REFERENCES users(user_id)
);

CREATE INDEX idx_incidents_monitor_id ON incidents(monitor_id);
CREATE INDEX idx_incidents_status ON incidents(status);
CREATE INDEX idx_incidents_start_time ON incidents(start_time DESC);
CREATE INDEX idx_incidents_organization_id ON incidents USING
    (
        (SELECT organization_id FROM monitors WHERE monitors.monitor_id = incidents.monitor_id)
    );

-- ============================================================================
-- INCIDENT_EVENTS (Timeline)
-- ============================================================================
CREATE TABLE incident_events (
    event_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    incident_id UUID NOT NULL REFERENCES incidents(incident_id) ON DELETE CASCADE,
    event_type VARCHAR(100) NOT NULL,
    message TEXT,
    created_by_user_id UUID REFERENCES users(user_id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_incident_events_incident_id ON incident_events(incident_id);

-- ============================================================================
-- ALERT_CHANNELS
-- ============================================================================
CREATE TABLE alert_channels (
    alert_channel_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    organization_id UUID NOT NULL REFERENCES organizations(organization_id),
    user_id UUID REFERENCES users(user_id),
    type VARCHAR(50) NOT NULL
        CHECK (type IN ('email', 'slack', 'teams', 'pagerduty', 'webhook', 'sms')),
    name VARCHAR(255) NOT NULL,
    
    -- Configuration (JSON, encrypted in application)
    configuration JSONB NOT NULL,
    
    is_active BOOLEAN DEFAULT TRUE,
    verification_status VARCHAR(50) NOT NULL DEFAULT 'pending'
        CHECK (verification_status IN ('pending', 'verified', 'failed')),
    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_alert_channels_organization_id ON alert_channels(organization_id);
CREATE INDEX idx_alert_channels_user_id ON alert_channels(user_id);
CREATE INDEX idx_alert_channels_type ON alert_channels(type);

-- ============================================================================
-- ALERT_POLICIES
-- ============================================================================
CREATE TABLE alert_policies (
    policy_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    monitor_id UUID NOT NULL REFERENCES monitors(monitor_id),
    alert_channel_id UUID NOT NULL REFERENCES alert_channels(alert_channel_id),
    
    condition VARCHAR(100) NOT NULL
        CHECK (condition IN ('down', 'slow', 'ssl_expiring')),
    threshold_ms INTEGER,
    
    is_enabled BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_alert_policies_monitor_id ON alert_policies(monitor_id);
CREATE INDEX idx_alert_policies_channel_id ON alert_policies(alert_channel_id);
CREATE UNIQUE INDEX idx_alert_policies_unique ON alert_policies(monitor_id, alert_channel_id, condition);

-- ============================================================================
-- ALERT_LOGS
-- ============================================================================
CREATE TABLE alert_logs (
    alert_log_id BIGSERIAL PRIMARY KEY,
    alert_channel_id UUID NOT NULL REFERENCES alert_channels(alert_channel_id),
    incident_id UUID REFERENCES incidents(incident_id),
    
    message_type VARCHAR(100),
    status VARCHAR(50) NOT NULL
        CHECK (status IN ('pending', 'sent', 'failed', 'bounced')),
    response_status VARCHAR(50),
    error_message TEXT,
    
    sent_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_alert_logs_channel_id ON alert_logs(alert_channel_id);
CREATE INDEX idx_alert_logs_incident_id ON alert_logs(incident_id);
CREATE INDEX idx_alert_logs_status ON alert_logs(status);
CREATE INDEX idx_alert_logs_created_at ON alert_logs(created_at DESC);

-- ============================================================================
-- STATUS_PAGES
-- ============================================================================
CREATE TABLE status_pages (
    page_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    organization_id UUID NOT NULL REFERENCES organizations(organization_id),
    
    name VARCHAR(255) NOT NULL,
    slug VARCHAR(100) NOT NULL,
    description TEXT,
    visibility VARCHAR(50) NOT NULL DEFAULT 'public'
        CHECK (visibility IN ('public', 'private', 'logged_in')),
    
    custom_domain VARCHAR(255),
    monitor_ids JSONB DEFAULT '[]'::jsonb,
    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    UNIQUE(organization_id, slug)
);

CREATE INDEX idx_status_pages_organization_id ON status_pages(organization_id);
CREATE INDEX idx_status_pages_custom_domain ON status_pages(custom_domain) WHERE custom_domain IS NOT NULL;

-- ============================================================================
-- STATUS_PAGE_UPDATES
-- ============================================================================
CREATE TABLE status_page_updates (
    update_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_id UUID NOT NULL REFERENCES status_pages(page_id),
    
    title VARCHAR(255) NOT NULL,
    description TEXT,
    status VARCHAR(50) NOT NULL
        CHECK (status IN ('investigating', 'monitoring', 'resolved')),
    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    resolved_at TIMESTAMP WITH TIME ZONE
);

CREATE INDEX idx_status_updates_page_id ON status_page_updates(page_id);

-- ============================================================================
-- WEBHOOKS
-- ============================================================================
CREATE TABLE webhooks (
    webhook_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    organization_id UUID NOT NULL REFERENCES organizations(organization_id),
    
    url TEXT NOT NULL,
    events JSONB NOT NULL DEFAULT '[]'::jsonb,
    secret TEXT, -- HMAC secret for signing
    
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_webhooks_organization_id ON webhooks(organization_id);

-- ============================================================================
-- WEBHOOK_LOGS
-- ============================================================================
CREATE TABLE webhook_logs (
    log_id BIGSERIAL PRIMARY KEY,
    webhook_id UUID NOT NULL REFERENCES webhooks(webhook_id),
    
    event_type VARCHAR(100),
    payload JSONB,
    response TEXT,
    response_code INTEGER,
    is_success BOOLEAN,
    
    retries_left INTEGER DEFAULT 5,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_webhook_logs_webhook_id ON webhook_logs(webhook_id);
CREATE INDEX idx_webhook_logs_success ON webhook_logs(is_success);

-- ============================================================================
-- AUDIT_LOGS
-- ============================================================================
CREATE TABLE audit_logs (
    log_id BIGSERIAL PRIMARY KEY,
    organization_id UUID NOT NULL REFERENCES organizations(organization_id),
    user_id UUID REFERENCES users(user_id),
    
    action VARCHAR(255) NOT NULL,
    resource_type VARCHAR(100),
    resource_id UUID,
    changes JSONB,
    
    ip_address INET,
    user_agent TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Partition by month for performance
-- CREATE TABLE audit_logs_2026_05 PARTITION OF audit_logs
--     FOR VALUES FROM ('2026-05-01') TO ('2026-06-01');

CREATE INDEX idx_audit_logs_organization_id ON audit_logs(organization_id);
CREATE INDEX idx_audit_logs_user_id ON audit_logs(user_id);
CREATE INDEX idx_audit_logs_resource ON audit_logs(resource_type, resource_id);
CREATE INDEX idx_audit_logs_created_at ON audit_logs(created_at DESC);

-- ============================================================================
-- BILLING
-- ============================================================================
CREATE TABLE billing (
    billing_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    organization_id UUID NOT NULL UNIQUE REFERENCES organizations(organization_id),
    
    plan VARCHAR(50) NOT NULL DEFAULT 'starter',
    billing_cycle VARCHAR(50) NOT NULL DEFAULT 'monthly',
    amount DECIMAL(10,2) NOT NULL,
    currency VARCHAR(3) DEFAULT 'USD',
    
    stripe_customer_id VARCHAR(255), -- Encrypted
    stripe_subscription_id VARCHAR(255), -- Encrypted
    
    status VARCHAR(50) NOT NULL DEFAULT 'active'
        CHECK (status IN ('active', 'cancelled', 'overdue')),
    
    next_billing_date TIMESTAMP WITH TIME ZONE,
    cancelled_at TIMESTAMP WITH TIME ZONE,
    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_billing_organization_id ON billing(organization_id);
CREATE INDEX idx_billing_status ON billing(status);

-- ============================================================================
-- INVOICES
-- ============================================================================
CREATE TABLE invoices (
    invoice_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    billing_id UUID NOT NULL REFERENCES billing(billing_id),
    
    amount DECIMAL(10,2) NOT NULL,
    period_start TIMESTAMP WITH TIME ZONE NOT NULL,
    period_end TIMESTAMP WITH TIME ZONE NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'draft',
    
    paid_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_invoices_billing_id ON invoices(billing_id);
CREATE INDEX idx_invoices_created_at ON invoices(created_at DESC);

-- ============================================================================
-- API_KEYS (For programmatic access)
-- ============================================================================
CREATE TABLE api_keys (
    key_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    organization_id UUID NOT NULL REFERENCES organizations(organization_id),
    user_id UUID NOT NULL REFERENCES users(user_id),
    
    key_hash VARCHAR(255) NOT NULL UNIQUE, -- SHA-256 hash
    name VARCHAR(255),
    
    permissions JSONB DEFAULT '[]'::jsonb,
    ip_whitelist JSONB DEFAULT '[]'::jsonb,
    
    last_used_at TIMESTAMP WITH TIME ZONE,
    expires_at TIMESTAMP WITH TIME ZONE,
    
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_api_keys_organization_id ON api_keys(organization_id);
CREATE INDEX idx_api_keys_user_id ON api_keys(user_id);
CREATE INDEX idx_api_keys_active ON api_keys(is_active);

-- ============================================================================
-- DATABASE CONSTRAINTS
-- ============================================================================

-- Add organization_id to audit logs via FK trigger if needed
-- Add organization_id to check_results via trigger for better performance

-- ============================================================================
-- VIEWS (For commonly used queries)
-- ============================================================================

-- Current uptime status for monitors
CREATE VIEW monitor_uptime_status AS
SELECT 
    m.monitor_id,
    m.organization_id,
    m.name,
    COUNT(CASE WHEN cr.is_success THEN 1 END)::FLOAT / 
    NULLIF(COUNT(*), 0) * 100 as uptime_percentage_24h,
    AVG(cr.response_time_ms) as avg_response_time,
    COUNT(DISTINCT DATE(cr.timestamp)) as check_days_count
FROM monitors m
LEFT JOIN check_results cr ON m.monitor_id = cr.monitor_id
    AND cr.timestamp > CURRENT_TIMESTAMP - INTERVAL '24 hours'
WHERE m.deleted_at IS NULL
GROUP BY m.monitor_id, m.organization_id, m.name;

-- Recent incidents for dashboard
CREATE VIEW recent_incidents AS
SELECT 
    i.incident_id,
    i.monitor_id,
    m.name as monitor_name,
    m.organization_id,
    i.status,
    i.start_time,
    i.end_time,
    EXTRACT(EPOCH FROM (COALESCE(i.end_time, CURRENT_TIMESTAMP) - i.start_time))::INTEGER as duration_seconds
FROM incidents i
JOIN monitors m ON i.monitor_id = m.monitor_id
WHERE i.created_at > CURRENT_TIMESTAMP - INTERVAL '30 days'
ORDER BY i.start_time DESC;
```

---

## 3. Indexing Strategy

### Indexing Rationale
```
Table               | Key Indexes                          | Reason
────────────────────┼─────────────────────────────────────┼─────────────────
organizations       | slug, status, created_at             | Frequent lookups
users               | org_id, email, status, role         | Auth & filtering
monitors            | org_id, team_id, status, created_at | Dashboard queries
check_results       | monitor_id, timestamp (composite)   | Time-series data
incidents           | monitor_id, status, start_time      | Incident searches
alert_channels      | org_id, type, user_id               | Notification routing
alert_policies      | monitor_id, channel_id              | Rule evaluation
alert_logs          | channel_id, status, created_at      | Audit trails
audit_logs          | org_id, user_id, created_at         | Compliance
api_keys            | org_id, user_id, active             | Auth validation
```

### Performance Optimization
```
1. Composite Indexes (B-tree)
   - (organization_id, created_at DESC) - Common filters
   - (monitor_id, timestamp DESC) - Time-series queries
   
2. Partial Indexes
   - api_keys WHERE is_active = true
   - monitors WHERE deleted_at IS NULL
   - users WHERE status = 'active'
   
3. JSONB Indexes
   - Tags on monitors (GIN index)
   - Configuration on alert_channels (GIN index)
   
4. Partitioning
   - check_results by month (for >1B rows)
   - audit_logs by month (for >1B rows)
```

---

## 4. Data Retention & Cleanup

```
Data Type                | Retention    | Cleanup Strategy
───────────────────────────────────────────────────────────
check_results            | 2 years      | Hourly batch delete
alert_logs               | 1 year       | Daily batch delete
audit_logs               | 6 months     | Monthly archive then delete
webhook_logs             | 30 days      | Daily delete
incident_events          | Indefinite   | Keep for history
incidents                | Indefinite   | Keep for reports
monitors                 | Soft delete  | Keep deleted_at timestamp
```

---

## Conclusion

This database design provides:
- **Scalability**: Partitioning for large tables, indexing for query performance
- **Multi-tenancy**: Organization-based data isolation
- **Auditability**: Complete audit trail for compliance
- **Performance**: Optimized indexes and views for common queries
- **Reliability**: Foreign key constraints and data integrity rules
