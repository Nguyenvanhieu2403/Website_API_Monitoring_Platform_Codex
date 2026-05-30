# Sprint 1: Authentication & Foundation

**Sprint Duration:** 2 weeks (10 business days)  
**Sprint Goal:** Implement core authentication system, user management, and organization foundation

---

## 1. Sprint Goal

Build a secure, production-ready authentication and authorization foundation that enables:

- User registration and login with JWT tokens
- Refresh token mechanism for session management
- Organization creation and management
- Role-based access control (Owner, Admin, Member, Read-Only)
- User profile management
- Secure password handling and token management

### Success Criteria

- ✅ Users can register with email and password
- ✅ Users can login and receive JWT tokens
- ✅ Refresh tokens work correctly for session extension
- ✅ Organizations can be created during registration
- ✅ Role-based permissions are enforced
- ✅ All endpoints are secured with proper authentication
- ✅ Code coverage > 80% for authentication module

---

## 2. User Stories

### US-001: User Registration

**As a** new user  
**I want to** register with my email and password  
**So that** I can create an account and access the monitoring platform

**Acceptance Criteria:**

- User can register with email, password, first name, and last name
- Email must be unique within an organization
- Password must meet complexity requirements (8+ chars, 1 uppercase, 1 number, 1 special char)
- User receives verification email after registration
- User is automatically assigned to an organization (new or existing)
- Registration returns JWT token for immediate access

**Story Points:** 5

---

### US-002: User Login

**As a** registered user  
**I want to** login with my credentials  
**So that** I can access my dashboard and monitoring features

**Acceptance Criteria:**

- User can login with email and password
- Invalid credentials show generic error message (security)
- Successful login returns access token and refresh token
- Last login timestamp is updated
- Failed login attempts are logged (rate limiting)
- JWT token expires after 15 minutes
- Refresh token expires after 7 days

**Story Points:** 3

---

### US-003: JWT Authentication

**As a** authenticated user  
**I want to** use JWT tokens for API access  
**So that** my requests are secure and stateless

**Acceptance Criteria:**

- Access tokens are valid for 15 minutes
- Refresh tokens are valid for 7 days
- Tokens contain user ID, organization ID, and roles
- Token validation occurs on every protected endpoint
- Expired tokens return 401 Unauthorized
- Invalid tokens return 401 Unauthorized
- Token refresh endpoint works correctly

**Story Points:** 5

---

### US-004: Refresh Token

**As a** user with an expired token  
**I want to** refresh my access token  
**So that** I can continue using the application without re-login

**Acceptance Criteria:**

- Refresh token endpoint accepts valid refresh tokens
- New access token is generated with same claims
- Refresh token is rotated (old one invalidated, new one issued)
- Compromised refresh tokens are revoked
- Refresh token can be revoked by user (logout)
- Refresh tokens are stored securely in database

**Story Points:** 3

---

### US-005: User Profile

**As a** user  
**I want to** view and update my profile  
**So that** I can manage my personal information

**Acceptance Criteria:**

- User can view their profile (name, email, role)
- User can update first name and last name
- User cannot change their email (security)
- User cannot change their own role (admin function)
- Profile changes are audited
- User can see their organization and team information

**Story Points:** 3

---

### US-006: Organization Management

**As a** user  
**I want to** create and manage organizations  
**So that** I can organize my monitoring resources

**Acceptance Criteria:**

- User can create an organization during registration
- Organization has a unique slug for identification
- Organization owner has full control
- Organization can have multiple teams
- Organization settings can be updated by owner/admin
- Organization can be deleted (soft delete)

**Story Points:** 5

---

### US-007: Roles & Permissions

**As a** system administrator  
**I want to** enforce role-based access control  
**So that** users only have access to appropriate features

**Acceptance Criteria:**

- Owner role has full access to organization
- Admin role can manage users and monitors but not billing
- Member role can create/edit/delete monitors
- Read-Only role can only view dashboards and reports
- Permissions are checked on every API endpoint
- Role hierarchy is enforced (Owner > Admin > Member > Read-Only)

**Story Points:** 5

---

## 3. Acceptance Criteria Summary

| Feature                 | Priority | Story Points | Status |
| ----------------------- | -------- | ------------ | ------ |
| User Registration       | High     | 5            | To Do  |
| User Login              | High     | 3            | To Do  |
| JWT Authentication      | High     | 5            | To Do  |
| Refresh Token           | High     | 3            | To Do  |
| User Profile            | Medium   | 3            | To Do  |
| Organization Management | High     | 5            | To Do  |
| Roles & Permissions     | High     | 5            | To Do  |
| **Total**               |          | **29**       |        |

---

## 4. Database Tables

### Users Table

```sql
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
    email_verified_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,

    UNIQUE(organization_id, email)
);

CREATE INDEX idx_users_organization_id ON users(organization_id);
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_status ON users(status);
```

### Organizations Table

```sql
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
    deleted_at TIMESTAMP WITH TIME ZONE
);

CREATE INDEX idx_organizations_slug ON organizations(slug);
CREATE INDEX idx_organizations_status ON organizations(status);
```

### Refresh Tokens Table

```sql
CREATE TABLE refresh_tokens (
    token_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    token_hash VARCHAR(255) NOT NULL UNIQUE,
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    revoked_at TIMESTAMP WITH TIME ZONE,
    replaced_by_token_id UUID REFERENCES refresh_tokens(token_id),
    remote_ip INET,
    user_agent TEXT
);

CREATE INDEX idx_refresh_tokens_user_id ON refresh_tokens(user_id);
CREATE INDEX idx_refresh_tokens_token_hash ON refresh_tokens(token_hash);
CREATE INDEX idx_refresh_tokens_expires_at ON refresh_tokens(expires_at);
```

### Teams Table

```sql
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
```

---

## 5. EF Core Entities

### User Entity

```csharp
namespace MonitoringPlatform.Domain.Entities;

public class User : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? TeamId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Member;
    public UserStatus Status { get; set; } = UserStatus.Active;
    public bool TwoFactorEnabled { get; set; }
    public string? TwoFactorSecret { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }

    // Navigation properties
    public Organization Organization { get; set; } = null!;
    public Team? Team { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();
}

public enum UserRole
{
    Owner = 1,
    Admin = 2,
    Member = 3,
    ReadOnly = 4
}

public enum UserStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3
}
```

### Organization Entity

```csharp
namespace MonitoringPlatform.Domain.Entities;

public class Organization : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public OrganizationStatus Status { get; set; } = OrganizationStatus.Active;
    public PlanType PlanType { get; set; } = PlanType.Starter;
    public int MaxMonitors { get; set; } = 10;
    public int MaxAlerts { get; set; } = 3;
    public string? BillingEmail { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public string? CustomDomain { get; set; }
    public string? LogoUrl { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Team> Teams { get; set; } = new List<Team>();

    // Computed properties
    public bool IsDeleted => DeletedAt.HasValue;
}

public enum OrganizationStatus
{
    Active = 1,
    Suspended = 2,
    Deleted = 3
}

public enum PlanType
{
    Starter = 1,
    Professional = 2,
    Enterprise = 3
}
```

### RefreshToken Entity

```csharp
namespace MonitoringPlatform.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid TokenId { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public Guid? ReplacedByTokenId { get; set; }
    public string? RemoteIp { get; set; }
    public string? UserAgent { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public RefreshToken? ReplacedByToken { get; set; }
    public ICollection<RefreshToken> ReplacedTokens { get; set; } = new List<RefreshToken>();

    // Computed properties
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => !IsRevoked && !IsExpired;
}
```

### Team Entity

```csharp
namespace MonitoringPlatform.Domain.Entities;

public class Team : BaseEntity
{
    public Guid TeamId { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation properties
    public Organization Organization { get; set; } = null!;
    public ICollection<User> Users { get; set; } = new List<User>();
}
```

---

## 6. Fluent Configurations

### User Configuration

```csharp
namespace MonitoringPlatform.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.UserId);

        builder.Property(u => u.UserId)
            .HasColumnName("user_id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(u => u.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(u => u.TeamId)
            .HasColumnName("team_id");

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100);

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .IsRequired();

        builder.Property(u => u.Role)
            .HasColumnName("role")
            .HasConversion<string>()
            .IsRequired()
            .HasDefaultValue(UserRole.Member);

        builder.Property(u => u.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired()
            .HasDefaultValue(UserStatus.Active);

        builder.Property(u => u.TwoFactorEnabled)
            .HasColumnName("two_factor_enabled")
            .HasDefaultValue(false);

        builder.Property(u => u.TwoFactorSecret)
            .HasColumnName("two_factor_secret");

        builder.Property(u => u.LastLoginAt)
            .HasColumnName("last_login_at");

        builder.Property(u => u.EmailVerifiedAt)
            .HasColumnName("email_verified_at");

        builder.HasIndex(u => u.OrganizationId)
            .HasDatabaseName("idx_users_organization_id");

        builder.HasIndex(u => u.Email)
            .HasDatabaseName("idx_users_email");

        builder.HasIndex(u => u.Status)
            .HasDatabaseName("idx_users_status");

        // Unique constraint for organization + email
        builder.HasIndex(u => new { u.OrganizationId, u.Email })
            .IsUnique();

        // Relationships
        builder.HasOne(u => u.Organization)
            .WithMany(o => o.Users)
            .HasForeignKey(u => u.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Team)
            .WithMany(t => t.Users)
            .HasForeignKey(u => u.TeamId)
            .OnDelete(DeleteBehavior.SetNull);

        // Global query filter for soft-deleted users
        builder.HasQueryFilter(u => u.Status != UserStatus.Inactive);
    }
}
```

### Organization Configuration

```csharp
namespace MonitoringPlatform.Infrastructure.Data.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("organizations");

        builder.HasKey(o => o.OrganizationId);

        builder.Property(o => o.OrganizationId)
            .HasColumnName("organization_id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(o => o.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(o => o.Slug)
            .HasColumnName("slug")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired()
            .HasDefaultValue(OrganizationStatus.Active);

        builder.Property(o => o.PlanType)
            .HasColumnName("plan_type")
            .HasConversion<string>()
            .IsRequired()
            .HasDefaultValue(PlanType.Starter);

        builder.Property(o => o.MaxMonitors)
            .HasColumnName("max_monitors")
            .HasDefaultValue(10);

        builder.Property(o => o.MaxAlerts)
            .HasColumnName("max_alerts")
            .HasDefaultValue(3);

        builder.Property(o => o.BillingEmail)
            .HasColumnName("billing_email")
            .HasMaxLength(255);

        builder.Property(o => o.TimeZone)
            .HasColumnName("time_zone")
            .HasDefaultValue("UTC");

        builder.Property(o => o.CustomDomain)
            .HasColumnName("custom_domain")
            .HasMaxLength(255);

        builder.Property(o => o.LogoUrl)
            .HasColumnName("logo_url");

        builder.Property(o => o.DeletedAt)
            .HasColumnName("deleted_at");

        builder.HasIndex(o => o.Slug)
            .IsUnique()
            .HasDatabaseName("idx_organizations_slug");

        builder.HasIndex(o => o.Status)
            .HasDatabaseName("idx_organizations_status");

        // Global query filter for soft-deleted organizations
        builder.HasQueryFilter(o => o.DeletedAt == null);
    }
}
```

### RefreshToken Configuration

```csharp
namespace MonitoringPlatform.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(r => r.TokenId);

        builder.Property(r => r.TokenId)
            .HasColumnName("token_id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(r => r.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(r => r.TokenHash)
            .HasColumnName("token_hash")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(r => r.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(r => r.RevokedAt)
            .HasColumnName("revoked_at");

        builder.Property(r => r.ReplacedByTokenId)
            .HasColumnName("replaced_by_token_id");

        builder.Property(r => r.RemoteIp)
            .HasColumnName("remote_ip");

        builder.Property(r => r.UserAgent)
            .HasColumnName("user_agent");

        builder.HasIndex(r => r.UserId)
            .HasDatabaseName("idx_refresh_tokens_user_id");

        builder.HasIndex(r => r.TokenHash)
            .IsUnique()
            .HasDatabaseName("idx_refresh_tokens_token_hash");

        builder.HasIndex(r => r.ExpiresAt)
            .HasDatabaseName("idx_refresh_tokens_expires_at");

        // Relationships
        builder.HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.ReplacedByToken)
            .WithMany(r => r.ReplacedTokens)
            .HasForeignKey(r => r.ReplacedByTokenId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
```

---

## 7. DTOs

### Authentication DTOs

```csharp
namespace MonitoringPlatform.Application.DTOs.Authentication;

public record RegisterRequestDto(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? OrganizationName = null
);

public record RegisterResponseDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    Guid OrganizationId,
    string OrganizationName,
    UserRole Role,
    AuthTokens Tokens
);

public record LoginRequestDto(
    string Email,
    string Password
);

public record LoginResponseDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    Guid OrganizationId,
    string OrganizationName,
    UserRole Role,
    AuthTokens Tokens
);

public record RefreshTokenRequestDto(
    string RefreshToken
);

public record RefreshTokenResponseDto(
    AuthTokens Tokens
);

public record AuthTokens(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt
);

public record RevokeTokenRequestDto(
    string? RefreshToken = null
);

public record LogoutResponseDto(
    bool Success,
    string Message
);
```

### User Profile DTOs

```csharp
namespace MonitoringPlatform.Application.DTOs.Users;

public record UserProfileResponseDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    UserRole Role,
    UserStatus Status,
    bool TwoFactorEnabled,
    DateTime? LastLoginAt,
    DateTime? EmailVerifiedAt,
    Guid OrganizationId,
    string OrganizationName,
    Guid? TeamId,
    string? TeamName
);

public record UpdateUserProfileRequestDto(
    string? FirstName = null,
    string? LastName = null
);

public record UpdateUserProfileResponseDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    DateTime UpdatedAt
);

public record ChangePasswordRequestDto(
    string CurrentPassword,
    string NewPassword
);

public record ChangePasswordResponseDto(
    bool Success,
    string Message
);
```

### Organization DTOs

```csharp
namespace MonitoringPlatform.Application.DTOs.Organizations;

public record OrganizationResponseDto(
    Guid OrganizationId,
    string Name,
    string Slug,
    OrganizationStatus Status,
    PlanType PlanType,
    int MaxMonitors,
    int MaxAlerts,
    string? BillingEmail,
    string TimeZone,
    string? CustomDomain,
    string? LogoUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateOrganizationRequestDto(
    string Name,
    string? Slug = null
);

public record CreateOrganizationResponseDto(
    Guid OrganizationId,
    string Name,
    string Slug,
    DateTime CreatedAt
);

public record UpdateOrganizationRequestDto(
    string? Name = null,
    string? BillingEmail = null,
    string? TimeZone = null,
    string? CustomDomain = null,
    string? LogoUrl = null
);

public record OrganizationMemberResponseDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    UserRole Role,
    UserStatus Status,
    DateTime CreatedAt
);
```

### Team DTOs

```csharp
namespace MonitoringPlatform.Application.DTOs.Teams;

public record TeamResponseDto(
    Guid TeamId,
    Guid OrganizationId,
    string Name,
    string? Description,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateTeamRequestDto(
    string Name,
    string? Description = null
);

public record CreateTeamResponseDto(
    Guid TeamId,
    string Name,
    DateTime CreatedAt
);

public record UpdateTeamRequestDto(
    string? Name = null,
    string? Description = null
);
```

### Common DTOs

```csharp
namespace MonitoringPlatform.Application.DTOs.Common;

public record PaginatedResult<T>(
    IReadOnlyCollection<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage
) where T : class;

public record ApiResponse<T>(
    T? Data,
    bool Success,
    string? Message,
    IEnumerable<string>? Errors
);

public record ErrorDetails(
    int StatusCode,
    string Message,
    string? Details,
    string? CorrelationId
);
```

---

## 8. Commands

### Authentication Commands

```csharp
namespace MonitoringPlatform.Application.Features.Authentication.Commands;

public record RegisterCommand : IRequest<RegisterResponseDto>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? OrganizationName { get; init; }
    public string? RemoteIp { get; init; }
    public string? UserAgent { get; init; }
}

public record LoginCommand : IRequest<LoginResponseDto>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string? RemoteIp { get; init; }
    public string? UserAgent { get; init; }
}

public record RefreshTokenCommand : IRequest<RefreshTokenResponseDto>
{
    public string RefreshToken { get; init; } = string.Empty;
    public string? RemoteIp { get; init; }
    public string? UserAgent { get; init; }
}

public record RevokeTokenCommand : IRequest<LogoutResponseDto>
{
    public Guid UserId { get; init; }
    public string? RefreshToken { get; init; }
}

public record LogoutCommand : IRequest<LogoutResponseDto>
{
    public Guid UserId { get; init; }
    public string? AllRefreshTokens { get; init; }
}
```

### User Commands

```csharp
namespace MonitoringPlatform.Application.Features.Users.Commands;

public record UpdateUserProfileCommand : IRequest<UpdateUserProfileResponseDto>
{
    public Guid UserId { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
}

public record ChangePasswordCommand : IRequest<ChangePasswordResponseDto>
{
    public Guid UserId { get; init; }
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}

public record InviteUserCommand : IRequest<InviteUserResponseDto>
{
    public Guid OrganizationId { get; init; }
    public Guid InvitedByUserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public Guid? TeamId { get; init; }
}

public record RemoveUserCommand : IRequest<Unit>
{
    public Guid OrganizationId { get; init; }
    public Guid TargetUserId { get; init; }
    public Guid RequestedByUserId { get; init; }
}

public record UpdateUserRoleCommand : IRequest<Unit>
{
    public Guid OrganizationId { get; init; }
    public Guid TargetUserId { get; init; }
    public Guid RequestedByUserId { get; init; }
    public UserRole NewRole { get; init; }
}
```

### Organization Commands

```csharp
namespace MonitoringPlatform.Application.Features.Organizations.Commands;

public record CreateOrganizationCommand : IRequest<CreateOrganizationResponseDto>
{
    public string Name { get; init; } = string.Empty;
    public string? Slug { get; init; }
    public Guid OwnerUserId { get; init; }
}

public record UpdateOrganizationCommand : IRequest<OrganizationResponseDto>
{
    public Guid OrganizationId { get; init; }
    public Guid RequestedByUserId { get; init; }
    public string? Name { get; init; }
    public string? BillingEmail { get; init; }
    public string? TimeZone { get; init; }
    public string? CustomDomain { get; init; }
    public string? LogoUrl { get; init; }
}

public record DeleteOrganizationCommand : IRequest<Unit>
{
    public Guid OrganizationId { get; init; }
    public Guid RequestedByUserId { get; init; }
}
```

### Team Commands

```csharp
namespace MonitoringPlatform.Application.Features.Teams.Commands;

public record CreateTeamCommand : IRequest<CreateTeamResponseDto>
{
    public Guid OrganizationId { get; init; }
    public Guid CreatedByUserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public record UpdateTeamCommand : IRequest<TeamResponseDto>
{
    public Guid TeamId { get; init; }
    public Guid OrganizationId { get; init; }
    public Guid UpdatedByUserId { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
}

public record DeleteTeamCommand : IRequest<Unit>
{
    public Guid TeamId { get; init; }
    public Guid OrganizationId { get; init; }
    public Guid DeletedByUserId { get; init; }
}
```

---

## 9. Queries

### Authentication Queries

```csharp
namespace MonitoringPlatform.Application.Features.Authentication.Queries;

public record ValidateTokenQuery : IRequest<bool>
{
    public string AccessToken { get; init; } = string.Empty;
}

public record GetCurrentUserQuery : IRequest<UserProfileResponseDto>
{
    public Guid UserId { get; init; }
}
```

### User Queries

```csharp
namespace MonitoringPlatform.Application.Features.Users.Queries;

public record GetUserByIdQuery : IRequest<UserProfileResponseDto>
{
    public Guid UserId { get; init; }
}

public record GetUserByEmailQuery : IRequest<UserProfileResponseDto?>
{
    public string Email { get; init; } = string.Empty;
    public Guid OrganizationId { get; init; }
}

public record GetOrganizationUsersQuery : IRequest<PaginatedResult<OrganizationMemberResponseDto>>
{
    public Guid OrganizationId { get; init; }
    public Guid RequestedByUserId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchTerm { get; init; }
    public UserRole? RoleFilter { get; init; }
    public UserStatus? StatusFilter { get; init; }
}
```

### Organization Queries

```csharp
namespace MonitoringPlatform.Application.Features.Organizations.Queries;

public record GetOrganizationByIdQuery : IRequest<OrganizationResponseDto>
{
    public Guid OrganizationId { get; init; }
    public Guid RequestedByUserId { get; init; }
}

public record GetOrganizationBySlugQuery : IRequest<OrganizationResponseDto?>
{
    public string Slug { get; init; } = string.Empty;
}

public record GetUserOrganizationQuery : IRequest<OrganizationResponseDto>
{
    public Guid UserId { get; init; }
}
```

### Team Queries

```csharp
namespace MonitoringPlatform.Application.Features.Teams.Queries;

public record GetTeamByIdQuery : IRequest<TeamResponseDto>
{
    public Guid TeamId { get; init; }
    public Guid OrganizationId { get; init; }
}

public record GetOrganizationTeamsQuery : IRequest<IReadOnlyCollection<TeamResponseDto>>
{
    public Guid OrganizationId { get; init; }
    public Guid RequestedByUserId { get; init; }
}

public record GetTeamMembersQuery : IRequest<IReadOnlyCollection<OrganizationMemberResponseDto>>
{
    public Guid TeamId { get; init; }
    public Guid OrganizationId { get; init; }
}
```

---

## 10. Handlers

### Authentication Handlers

```csharp
namespace MonitoringPlatform.Application.Features.Authentication.Handlers;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IOrganizationRepository organizationRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHashingService passwordHashingService,
        IEmailService emailService,
        IMapper mapper,
        ILogger<RegisterCommandHandler> logger)
    {
        _userRepository = userRepository;
        _organizationRepository = organizationRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHashingService = passwordHashingService;
        _emailService = emailService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<RegisterResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new DuplicateException($"User with email {request.Email} already exists");
        }

        // Create or get organization
        var organization = await GetOrCreateOrganizationAsync(request);

        // Create user
        var user = new User
        {
            UserId = Guid.NewGuid(),
            OrganizationId = organization.OrganizationId,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = _passwordHashingService.HashPassword(request.Password),
            Role = UserRole.Owner, // First user is owner
            Status = UserStatus.Active,
            EmailVerifiedAt = null
        };

        await _userRepository.CreateAsync(user);

        // Generate tokens
        var tokens = await _jwtTokenService.GenerateTokensAsync(user);

        // Store refresh token
        await _refreshTokenRepository.CreateAsync(new RefreshToken
        {
            TokenId = Guid.NewGuid(),
            UserId = user.UserId,
            TokenHash = _passwordHashingService.HashToken(tokens.RefreshToken),
            ExpiresAt = tokens.RefreshTokenExpiresAt,
            RemoteIp = request.RemoteIp,
            UserAgent = request.UserAgent
        });

        // Send verification email
        await _emailService.SendEmailVerificationAsync(user.Email, user.UserId);

        _logger.LogInformation("User {UserId} registered successfully", user.UserId);

        return new RegisterResponseDto(
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            organization.OrganizationId,
            organization.Name,
            user.Role,
            tokens
        );
    }

    private async Task<Organization> GetOrCreateOrganizationAsync(RegisterCommand request)
    {
        var orgName = request.OrganizationName ?? $"{request.FirstName}'s Organization";
        var orgSlug = request.OrganizationName?.ToSlug() ?? $"{request.FirstName}-{Guid.NewGuid():N}".ToSlug()[..20];

        var organization = new Organization
        {
            OrganizationId = Guid.NewGuid(),
            Name = orgName,
            Slug = orgSlug,
            Status = OrganizationStatus.Active,
            PlanType = PlanType.Starter,
            MaxMonitors = 10,
            MaxAlerts = 3
        };

        await _organizationRepository.CreateAsync(organization);
        return organization;
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IMapper _mapper;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHashingService passwordHashingService,
        IMapper mapper,
        ILogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHashingService = passwordHashingService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            // Generic error message for security
            throw new UnauthorizedException("Invalid email or password");
        }

        // Verify password
        if (!_passwordHashingService.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
            throw new UnauthorizedException("Invalid email or password");
        }

        // Check user status
        if (user.Status != UserStatus.Active)
        {
            throw new ForbiddenException("Account is not active");
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        // Generate tokens
        var tokens = await _jwtTokenService.GenerateTokensAsync(user);

        // Store refresh token
        await _refreshTokenRepository.CreateAsync(new RefreshToken
        {
            TokenId = Guid.NewGuid(),
            UserId = user.UserId,
            TokenHash = _passwordHashingService.HashToken(tokens.RefreshToken),
            ExpiresAt = tokens.RefreshTokenExpiresAt,
            RemoteIp = request.RemoteIp,
            UserAgent = request.UserAgent
        });

        _logger.LogInformation("User {UserId} logged in successfully", user.UserId);

        return new LoginResponseDto(
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            user.OrganizationId,
            user.Organization.Name,
            user.Role,
            tokens
        );
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponseDto>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHashingService passwordHashingService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHashingService = passwordHashingService;
        _logger = logger;
    }

    public async Task<RefreshTokenResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Find refresh token by hash
        var tokenHash = _passwordHashingService.HashToken(request.RefreshToken);
        var refreshToken = await _refreshTokenRepository.GetByHashAsync(tokenHash);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            throw new UnauthorizedException("Invalid or expired refresh token");
        }

        // Get user
        var user = await _userRepository.GetByIdAsync(refreshToken.UserId);
        if (user == null || user.Status != UserStatus.Active)
        {
            throw new UnauthorizedException("User not found or inactive");
        }

        // Revoke old token
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RemoteIp = request.RemoteIp;
        refreshToken.UserAgent = request.UserAgent;

        // Generate new tokens
        var tokens = await _jwtTokenService.GenerateTokensAsync(user);

        // Create new refresh token
        var newRefreshToken = new RefreshToken
        {
            TokenId = Guid.NewGuid(),
            UserId = user.UserId,
            TokenHash = _passwordHashingService.HashToken(tokens.RefreshToken),
            ExpiresAt = tokens.RefreshTokenExpiresAt,
            RemoteIp = request.RemoteIp,
            UserAgent = request.UserAgent,
            ReplacedByTokenId = refreshToken.TokenId
        };

        await _refreshTokenRepository.CreateAsync(newRefreshToken);

        _logger.LogInformation("Token refreshed for user {UserId}", user.UserId);

        return new RefreshTokenResponseDto(tokens);
    }
}
```

### User Handlers

```csharp
namespace MonitoringPlatform.Application.Features.Users.Handlers;

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, UpdateUserProfileResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateUserProfileCommandHandler> _logger;

    public UpdateUserProfileCommandHandler(
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<UpdateUserProfileCommandHandler> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UpdateUserProfileResponseDto> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }

        // Update fields
        if (!string.IsNullOrWhiteSpace(request.FirstName))
        {
            user.FirstName = request.FirstName;
        }
        if (!string.IsNullOrWhiteSpace(request.LastName))
        {
            user.LastName = request.LastName;
        }

        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("User {UserId} profile updated", request.UserId);

        return new UpdateUserProfileResponseDto(
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            user.FullName,
            DateTime.UtcNow
        );
    }
}

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ChangePasswordResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHashingService passwordHashingService,
        ILogger<ChangePasswordCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHashingService = passwordHashingService;
        _logger = logger;
    }

    public async Task<ChangePasswordResponseDto> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }

        // Verify current password
        if (!_passwordHashingService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedException("Current password is incorrect");
        }

        // Update password
        user.PasswordHash = _passwordHashingService.HashPassword(request.NewPassword);
        await _userRepository.UpdateAsync(user);

        // Revoke all refresh tokens (force re-login)
        await _userRepository.RevokeAllRefreshTokensAsync(user.UserId);

        _logger.LogInformation("User {UserId} changed password", request.UserId);

        return new ChangePasswordResponseDto(true, "Password changed successfully");
    }
}
```

---

## 11. Services

### IJwtTokenService

```csharp
namespace MonitoringPlatform.Application.Interfaces;

public interface IJwtTokenService
{
    Task<AuthTokens> GenerateTokensAsync(User user);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Task RevokeTokenAsync(Guid tokenId);
    Task RevokeAllTokensAsync(Guid userId);
}
```

### JwtTokenService Implementation

```csharp
namespace MonitoringPlatform.Infrastructure.Security;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly SymmetricSecurityKey _signingKey;

    public JwtTokenService(
        IOptions<JwtSettings> jwtSettings,
        IUserRepository userRepository,
        IPasswordHashingService passwordHashingService)
    {
        _jwtSettings = jwtSettings.Value;
        _userRepository = userRepository;
        _passwordHashingService = passwordHashingService;
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
    }

    public async Task<AuthTokens> GenerateTokensAsync(User user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        return new AuthTokens(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
        );
    }

    public string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("organization_id", user.OrganizationId.ToString()),
            new Claim("role", user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.ValidIssuer,
            audience: _jwtSettings.ValidAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _signingKey,
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }

    public async Task RevokeTokenAsync(Guid tokenId)
    {
        var token = await _userRepository.GetRefreshTokenAsync(tokenId);
        if (token != null)
        {
            token.RevokedAt = DateTime.UtcNow;
            await _userRepository.UpdateRefreshTokenAsync(token);
        }
    }

    public async Task RevokeAllTokensAsync(Guid userId)
    {
        await _userRepository.RevokeAllRefreshTokensAsync(userId);
    }
}
```

### IPasswordHashingService

```csharp
namespace MonitoringPlatform.Application.Interfaces;

public interface IPasswordHashingService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
    string HashToken(string token);
}
```

### PasswordHashingService Implementation

```csharp
namespace MonitoringPlatform.Infrastructure.Security;

public class PasswordHashingService : IPasswordHashingService
{
    public string HashPassword(string password)
    {
        // Using BCrypt for password hashing
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }

    public string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
```

### IEmailService

```csharp
namespace MonitoringPlatform.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string email, Guid userId);
    Task SendWelcomeEmailAsync(string email, string userName);
    Task SendPasswordResetEmailAsync(string email, string resetToken);
}
```

---

## 12. Repositories

### IUserRepository

```csharp
namespace MonitoringPlatform.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId);
    Task<User?> GetByEmailAsync(string email, Guid? organizationId = null);
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid userId);
    Task<IEnumerable<User>> GetByOrganizationAsync(Guid organizationId);
    Task<IEnumerable<User>> GetByTeamAsync(Guid teamId);
    Task<RefreshToken?> GetRefreshTokenAsync(Guid tokenId);
    Task UpdateRefreshTokenAsync(RefreshToken token);
    Task RevokeAllRefreshTokensAsync(Guid userId);
}
```

### UserRepository Implementation

```csharp
namespace MonitoringPlatform.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(ApplicationDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(Guid userId)
    {
        return await _context.Users
            .Include(u => u.Organization)
            .Include(u => u.Team)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<User?> GetByEmailAsync(string email, Guid? organizationId = null)
    {
        var query = _context.Users
            .Include(u => u.Organization)
            .AsQueryable();

        if (organizationId.HasValue)
        {
            query = query.Where(u => u.OrganizationId == organizationId.Value);
        }

        return await query.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> CreateAsync(User user)
    {
        user.UserId = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} created", user.UserId);
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} updated", user.UserId);
    }

    public async Task DeleteAsync(Guid userId)
    {
        var user = await GetByIdAsync(userId);
        if (user != null)
        {
            user.Status = UserStatus.Inactive;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} deleted", userId);
        }
    }

    public async Task<IEnumerable<User>> GetByOrganizationAsync(Guid organizationId)
    {
        return await _context.Users
            .Include(u => u.Team)
            .Where(u => u.OrganizationId == organizationId)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetByTeamAsync(Guid teamId)
    {
        return await _context.Users
            .Where(u => u.TeamId == teamId)
            .ToListAsync();
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(Guid tokenId)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.TokenId == tokenId);
    }

    public async Task UpdateRefreshTokenAsync(RefreshToken token)
    {
        _context.RefreshTokens.Update(token);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeAllRefreshTokensAsync(Guid userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(r => r.UserId == userId && r.RevokedAt == null)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}
```

### IOrganizationRepository

```csharp
namespace MonitoringPlatform.Domain.Interfaces;

public interface IOrganizationRepository
{
    Task<Organization?> GetByIdAsync(Guid organizationId);
    Task<Organization?> GetBySlugAsync(string slug);
    Task<Organization> CreateAsync(Organization organization);
    Task UpdateAsync(Organization organization);
    Task DeleteAsync(Guid organizationId);
    Task<IEnumerable<Organization>> GetByUserAsync(Guid userId);
}
```

### IRefreshTokenRepository

```csharp
namespace MonitoringPlatform.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByIdAsync(Guid tokenId);
    Task<RefreshToken?> GetByHashAsync(string tokenHash);
    Task<RefreshToken> CreateAsync(RefreshToken token);
    Task RevokeAsync(Guid tokenId);
    Task RevokeAllAsync(Guid userId);
    Task<IEnumerable<RefreshToken>> GetActiveTokensAsync(Guid userId);
}
```

---

## 13. API Endpoints

### Authentication Endpoints

```
POST   /api/v1/auth/register          - Register new user
POST   /api/v1/auth/login             - Login user
POST   /api/v1/auth/refresh           - Refresh access token
POST   /api/v1/auth/revoke            - Revoke refresh token
POST   /api/v1/auth/logout            - Logout user (revoke all tokens)
POST   /api/v1/auth/forgot-password   - Request password reset
POST   /api/v1/auth/reset-password    - Reset password with token
POST   /api/v1/auth/verify-email      - Verify email address
GET    /api/v1/auth/me                - Get current user profile
```

### User Endpoints

```
GET    /api/v1/users/profile          - Get user profile
PUT    /api/v1/users/profile          - Update user profile
PUT    /api/v1/users/password         - Change password
GET    /api/v1/users/{userId}         - Get user by ID (Admin)
```

### Organization Endpoints

```
GET    /api/v1/organizations/current  - Get current user's organization
GET    /api/v1/organizations/{orgId}  - Get organization by ID
PUT    /api/v1/organizations/{orgId}  - Update organization (Owner/Admin)
GET    /api/v1/organizations/{orgId}/members  - Get organization members
POST   /api/v1/organizations/{orgId}/invite   - Invite user to organization
DELETE /api/v1/organizations/{orgId}/members/{userId}  - Remove user
PUT    /api/v1/organizations/{orgId}/members/{userId}/role  - Update user role
```

### Team Endpoints

```
GET    /api/v1/teams                  - Get teams in organization
POST   /api/v1/teams                  - Create team
GET    /api/v1/teams/{teamId}         - Get team details
PUT    /api/v1/teams/{teamId}         - Update team
DELETE /api/v1/teams/{teamId}         - Delete team
GET    /api/v1/teams/{teamId}/members - Get team members
```

---

## 14. Swagger Contracts

### Register Endpoint

```yaml
/register:
  post:
    tags:
      - Authentication
    summary: Register a new user
    description: Creates a new user account and returns JWT tokens
    requestBody:
      required: true
      content:
        application/json:
          schema:
            type: object
            required:
              - email
              - password
              - firstName
              - lastName
            properties:
              email:
                type: string
                format: email
                example: "john@example.com"
              password:
                type: string
                minLength: 8
                example: "SecurePass123!"
              firstName:
                type: string
                example: "John"
              lastName:
                type: string
                example: "Doe"
              organizationName:
                type: string
                example: "Acme Corp"
    responses:
      201:
        description: User registered successfully
        content:
          application/json:
            schema:
              $ref: "#/components/schemas/RegisterResponse"
      400:
        description: Invalid request
      409:
        description: Email already exists
```

### Login Endpoint

```yaml
/login:
  post:
    tags:
      - Authentication
    summary: Login user
    description: Authenticates user and returns JWT tokens
    requestBody:
      required: true
      content:
        application/json:
          schema:
            type: object
            required:
              - email
              - password
            properties:
              email:
                type: string
                format: email
              password:
                type: string
                format: password
    responses:
      200:
        description: Login successful
        content:
          application/json:
            schema:
              $ref: "#/components/schemas/LoginResponse"
      401:
        description: Invalid credentials
```

### Components Schema

```yaml
components:
  schemas:
    RegisterResponse:
      type: object
      properties:
        userId:
          type: string
          format: uuid
        email:
          type: string
        firstName:
          type: string
        lastName:
          type: string
        organizationId:
          type: string
          format: uuid
        organizationName:
          type: string
        role:
          type: string
          enum: [Owner, Admin, Member, ReadOnly]
        tokens:
          $ref: "#/components/schemas/AuthTokens"

    AuthTokens:
      type: object
      properties:
        accessToken:
          type: string
        refreshToken:
          type: string
        accessTokenExpiresAt:
          type: string
          format: date-time
        refreshTokenExpiresAt:
          type: string
          format: date-time

    LoginResponse:
      type: object
      properties:
        userId:
          type: string
          format: uuid
        email:
          type: string
        firstName:
          type: string
        lastName:
          type: string
        organizationId:
          type: string
          format: uuid
        organizationName:
          type: string
        role:
          type: string
        tokens:
          $ref: "#/components/schemas/AuthTokens"

    ErrorDetails:
      type: object
      properties:
        statusCode:
          type: integer
        message:
          type: string
        details:
          type: string
        correlationId:
          type: string
```

---

## 15. Validation Rules

### Registration Validator

```csharp
namespace MonitoringPlatform.Application.Validators.Authentication;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(128).WithMessage("Password must not exceed 128 characters")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]").WithMessage(
                "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.OrganizationName)
            .MaximumLength(255).WithMessage("Organization name must not exceed 255 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.OrganizationName));
    }
}
```

### Login Validator

```csharp
namespace MonitoringPlatform.Application.Validators.Authentication;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}
```

### Update Profile Validator

```csharp
namespace MonitoringPlatform.Application.Validators.Users;

public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));
    }
}
```

### Change Password Validator

```csharp
namespace MonitoringPlatform.Application.Validators.Users;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("New password must be at least 8 characters")
            .MaximumLength(128).WithMessage("New password must not exceed 128 characters")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]").WithMessage(
                "New password must contain at least one uppercase letter, one lowercase letter, one number, and one special character");

        RuleFor(x => x.NewPassword)
            .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from current password");
    }
}
```

---

## 16. Folder Structure

### Backend Project Structure

```
src/
├── MonitoringPlatform.sln
├── MonitoringPlatform.Domain/
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Organization.cs
│   │   ├── RefreshToken.cs
│   │   ├── Team.cs
│   │   └── BaseEntity.cs
│   ├── Enums/
│   │   ├── UserRole.cs
│   │   ├── UserStatus.cs
│   │   ├── OrganizationStatus.cs
│   │   └── PlanType.cs
│   ├── Interfaces/
│   │   ├── IUserRepository.cs
│   │   ├── IOrganizationRepository.cs
│   │   ├── IRefreshTokenRepository.cs
│   │   └── ITeamRepository.cs
│   ├── Events/
│   │   ├── UserRegisteredEvent.cs
│   │   ├── UserLoggedInEvent.cs
│   │   └── OrganizationCreatedEvent.cs
│   └── MonitoringPlatform.Domain.csproj
│
├── MonitoringPlatform.Application/
│   ├── DTOs/
│   │   ├── Authentication/
│   │   │   ├── RegisterRequestDto.cs
│   │   │   ├── RegisterResponseDto.cs
│   │   │   ├── LoginRequestDto.cs
│   │   │   ├── LoginResponseDto.cs
│   │   │   └── AuthTokens.cs
│   │   ├── Users/
│   │   │   ├── UserProfileResponseDto.cs
│   │   │   └── UpdateUserProfileRequestDto.cs
│   │   ├── Organizations/
│   │   │   ├── OrganizationResponseDto.cs
│   │   │   └── CreateOrganizationRequestDto.cs
│   │   └── Teams/
│   │       ├── TeamResponseDto.cs
│   │       └── CreateTeamRequestDto.cs
│   ├── Features/
│   │   ├── Authentication/
│   │   │   ├── Commands/
│   │   │   │   ├── RegisterCommand.cs
│   │   │   │   ├── LoginCommand.cs
│   │   │   │   └── RefreshTokenCommand.cs
│   │   │   ├── Queries/
│   │   │   │   └── ValidateTokenQuery.cs
│   │   │   └── Handlers/
│   │   │       ├── RegisterCommandHandler.cs
│   │   │       ├── LoginCommandHandler.cs
│   │   │       └── RefreshTokenCommandHandler.cs
│   │   ├── Users/
│   │   │   ├── Commands/
│   │   │   ├── Queries/
│   │   │   └── Handlers/
│   │   ├── Organizations/
│   │   │   ├── Commands/
│   │   │   ├── Queries/
│   │   │   └── Handlers/
│   │   └── Teams/
│   │       ├── Commands/
│   │       ├── Queries/
│   │       └── Handlers/
│   ├── Validators/
│   │   ├── Authentication/
│   │   │   ├── RegisterCommandValidator.cs
│   │   │   └── LoginCommandValidator.cs
│   │   └── Users/
│   │       ├── UpdateProfileValidator.cs
│   │       └── ChangePasswordValidator.cs
│   ├── Interfaces/
│   │   ├── IJwtTokenService.cs
│   │   ├── IPasswordHashingService.cs
│   │   ├── IEmailService.cs
│   │   └── ICurrentUserService.cs
│   ├── Mappings/
│   │   └── MappingProfile.cs
│   ├── Behaviors/
│   │   ├── ValidationBehavior.cs
│   │   ├── LoggingBehavior.cs
│   │   └── PerformanceBehavior.cs
│   ├── Exceptions/
│   │   ├── ApiException.cs
│   │   ├── ValidationException.cs
│   │   ├── NotFoundException.cs
│   │   ├── UnauthorizedException.cs
│   │   └── DuplicateException.cs
│   └── MonitoringPlatform.Application.csproj
│
├── MonitoringPlatform.Infrastructure/
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   ├── Configurations/
│   │   │   ├── UserConfiguration.cs
│   │   │   ├── OrganizationConfiguration.cs
│   │   │   ├── RefreshTokenConfiguration.cs
│   │   │   └── TeamConfiguration.cs
│   │   └── Migrations/
│   │       └── [Migration files]
│   ├── Repositories/
│   │   ├── UserRepository.cs
│   │   ├── OrganizationRepository.cs
│   │   ├── RefreshTokenRepository.cs
│   │   ├── TeamRepository.cs
│   │   └── UnitOfWork.cs
│   ├── Security/
│   │   ├── JwtTokenService.cs
│   │   ├── PasswordHashingService.cs
│   │   └── JwtSettings.cs
│   ├── Services/
│   │   ├── EmailService.cs
│   │   ├── CurrentUserService.cs
│   │   └── DateTimeService.cs
│   ├── DependencyInjection.cs
│   └── MonitoringPlatform.Infrastructure.csproj
│
├── MonitoringPlatform.API/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── UsersController.cs
│   │   ├── OrganizationsController.cs
│   │   └── TeamsController.cs
│   ├── Middleware/
│   │   ├── ExceptionHandlingMiddleware.cs
│   │   ├── RequestLoggingMiddleware.cs
│   │   └── JwtMiddleware.cs
│   ├── Extensions/
│   │   ├── ServiceCollectionExtensions.cs
│   │   └── ApplicationBuilderExtensions.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Program.cs
│   └── MonitoringPlatform.API.csproj
│
└── MonitoringPlatform.Shared/
    ├── Constants/
    │   └── ApplicationConstants.cs
    ├── Helpers/
    │   └── SlugHelper.cs
    └── MonitoringPlatform.Shared.csproj

tests/
├── MonitoringPlatform.Domain.Tests/
│   └── Entities/
│       └── UserTests.cs
├── MonitoringPlatform.Application.Tests/
│   ├── Handlers/
│   │   ├── RegisterCommandHandlerTests.cs
│   │   └── LoginCommandHandlerTests.cs
│   └── Validators/
│       ├── RegisterCommandValidatorTests.cs
│       └── LoginCommandValidatorTests.cs
├── MonitoringPlatform.Infrastructure.Tests/
│   └── Services/
│       ├── JwtTokenServiceTests.cs
│       └── PasswordHashingServiceTests.cs
└── MonitoringPlatform.API.Tests/
    └── Endpoints/
        ├── AuthEndpointTests.cs
        └── UserEndpointTests.cs
```

---

## 17. Angular Architecture

### Project Structure

```
src/
├── app/
│   ├── core/
│   │   ├── auth/
│   │   │   ├── guards/
│   │   │   │   ├── auth.guard.ts
│   │   │   │   └── guest.guard.ts
│   │   │   ├── interceptors/
│   │   │   │   ├── jwt.interceptor.ts
│   │   │   │   └── error.interceptor.ts
│   │   │   ├── models/
│   │   │   │   ├── user.model.ts
│   │   │   │   ├── auth-tokens.model.ts
│   │   │   │   └── login-request.model.ts
│   │   │   ├── services/
│   │   │   │   ├── auth.service.ts
│   │   │   │   └── token.service.ts
│   │   │   └── auth.module.ts
│   │   ├── services/
│   │   │   ├── api.service.ts
│   │   │   ├── organization.service.ts
│   │   │   └── team.service.ts
│   │   ├── models/
│   │   │   ├── organization.model.ts
│   │   │   ├── team.model.ts
│   │   │   └── api-response.model.ts
│   │   └── core.module.ts
│   │
│   ├── shared/
│   │   ├── components/
│   │   │   ├── header/
│   │   │   ├── sidebar/
│   │   │   ├── loading/
│   │   │   └── error/
│   │   ├── directives/
│   │   │   └── has-permission.directive.ts
│   │   ├── pipes/
│   │   │   └── role.pipe.ts
│   │   └── shared.module.ts
│   │
│   ├── features/
│   │   ├── auth/
│   │   │   ├── login/
│   │   │   │   ├── login.component.ts
│   │   │   │   ├── login.component.html
│   │   │   │   ├── login.component.scss
│   │   │   │   └── login.component.spec.ts
│   │   │   ├── register/
│   │   │   │   ├── register.component.ts
│   │   │   │   ├── register.component.html
│   │   │   │   ├── register.component.scss
│   │   │   │   └── register.component.spec.ts
│   │   │   ├── forgot-password/
│   │   │   └── auth-routing.module.ts
│   │   │
│   │   ├── dashboard/
│   │   │   ├── dashboard.component.ts
│   │   │   ├── dashboard.component.html
│   │   │   ├── dashboard.component.scss
│   │   │   └── dashboard.module.ts
│   │   │
│   │   ├── profile/
│   │   │   ├── profile.component.ts
│   │   │   ├── profile.component.html
│   │   │   └── profile.module.ts
│   │   │
│   │   └── organizations/
│   │       ├── organization-settings/
│   │       └── organization.module.ts
│   │
│   ├── app-routing.module.ts
│   ├── app.component.ts
│   ├── app.component.html
│   └── app.module.ts
│
├── environments/
│   ├── environment.ts
│   └── environment.prod.ts
│
└── assets/
    ├── images/
    └── i18n/
```

---

## 18. Angular Pages

### Login Page

```typescript
// login.component.ts
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { Router } from "@angular/router";
import { AuthService } from "@core/auth/services/auth.service";
import { LoginRequest } from "@core/auth/models/login-request.model";

@Component({
  selector: "app-login",
  templateUrl: "./login.component.html",
  styleUrls: ["./login.component.scss"],
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  isLoading = false;
  errorMessage = "";

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.loginForm = this.fb.group({
      email: ["", [Validators.required, Validators.email]],
      password: ["", [Validators.required]],
    });
  }

  get f() {
    return this.loginForm.controls;
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = "";

    const loginRequest: LoginRequest = {
      email: this.f.email.value,
      password: this.f.password.value,
    };

    this.authService.login(loginRequest).subscribe({
      next: (response) => {
        this.router.navigate(["/dashboard"]);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || "Login failed";
        this.isLoading = false;
      },
    });
  }
}
```

### Register Page

```typescript
// register.component.ts
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { Router } from "@angular/router";
import { AuthService } from "@core/auth/services/auth.service";
import { RegisterRequest } from "@core/auth/models/register-request.model";

@Component({
  selector: "app-register",
  templateUrl: "./register.component.html",
  styleUrls: ["./register.component.scss"],
})
export class RegisterComponent implements OnInit {
  registerForm!: FormGroup;
  isLoading = false;
  errorMessage = "";

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.registerForm = this.fb.group(
      {
        email: ["", [Validators.required, Validators.email]],
        password: [
          "",
          [
            Validators.required,
            Validators.minLength(8),
            Validators.pattern(
              /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])/,
            ),
          ],
        ],
        confirmPassword: ["", Validators.required],
        firstName: ["", [Validators.required, Validators.maxLength(100)]],
        lastName: ["", [Validators.required, Validators.maxLength(100)]],
        organizationName: [""],
      },
      { validators: this.passwordMatchValidator },
    );
  }

  passwordMatchValidator(form: FormGroup) {
    return form.get("password")?.value === form.get("confirmPassword")?.value
      ? null
      : { mismatch: true };
  }

  get f() {
    return this.registerForm.controls;
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = "";

    const registerRequest: RegisterRequest = {
      email: this.f.email.value,
      password: this.f.password.value,
      firstName: this.f.firstName.value,
      lastName: this.f.lastName.value,
      organizationName: this.f.organizationName.value || undefined,
    };

    this.authService.register(registerRequest).subscribe({
      next: (response) => {
        this.router.navigate(["/dashboard"]);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || "Registration failed";
        this.isLoading = false;
      },
    });
  }
}
```

### Profile Page

```typescript
// profile.component.ts
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { AuthService } from "@core/auth/services/auth.service";
import { User } from "@core/auth/models/user.model";

@Component({
  selector: "app-profile",
  templateUrl: "./profile.component.html",
  styleUrls: ["./profile.component.scss"],
})
export class ProfileComponent implements OnInit {
  profileForm!: FormGroup;
  user: User | null = null;
  isLoading = false;
  message = "";

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
  ) {}

  ngOnInit(): void {
    this.profileForm = this.fb.group({
      firstName: ["", Validators.maxLength(100)],
      lastName: ["", Validators.maxLength(100)],
    });

    this.loadUserProfile();
  }

  loadUserProfile(): void {
    this.authService.getCurrentUser().subscribe({
      next: (user) => {
        this.user = user;
        this.profileForm.patchValue({
          firstName: user.firstName,
          lastName: user.lastName,
        });
      },
    });
  }

  onSubmit(): void {
    if (this.profileForm.invalid) {
      return;
    }

    this.isLoading = true;
    this.message = "";

    this.authService
      .updateProfile({
        firstName: this.profileForm.value.firstName,
        lastName: this.profileForm.value.lastName,
      })
      .subscribe({
        next: (response) => {
          this.message = "Profile updated successfully";
          this.isLoading = false;
        },
        error: (error) => {
          this.message = error.error?.message || "Failed to update profile";
          this.isLoading = false;
        },
      });
  }
}
```

---

## 19. Angular Services

### Auth Service

```typescript
// auth.service.ts
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { BehaviorSubject, Observable, throwError } from "rxjs";
import { map, catchError, tap } from "rxjs/operators";
import { environment } from "@environments/environment";
import { TokenService } from "./token.service";
import { User } from "../models/user.model";
import { AuthTokens } from "../models/auth-tokens.model";
import { LoginRequest } from "../models/login-request.model";
import { RegisterRequest } from "../models/register-request.model";

@Injectable({
  providedIn: "root",
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private http: HttpClient,
    private tokenService: TokenService,
  ) {
    this.loadCurrentUser();
  }

  get currentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  get isAuthenticated(): boolean {
    return this.tokenService.hasValidToken();
  }

  login(request: LoginRequest): Observable<AuthTokens> {
    return this.http
      .post<AuthTokens>(`${this.apiUrl}/login`, request)
      .pipe(tap((response) => this.handleAuthResponse(response)));
  }

  register(request: RegisterRequest): Observable<AuthTokens> {
    return this.http
      .post<AuthTokens>(`${this.apiUrl}/register`, request)
      .pipe(tap((response) => this.handleAuthResponse(response)));
  }

  logout(): Observable<void> {
    return this.http
      .post<void>(`${this.apiUrl}/logout`, {})
      .pipe(tap(() => this.clearAuth()));
  }

  refreshToken(): Observable<AuthTokens> {
    const refreshToken = this.tokenService.getRefreshToken();
    return this.http
      .post<AuthTokens>(`${this.apiUrl}/refresh`, { refreshToken })
      .pipe(tap((response) => this.handleAuthResponse(response)));
  }

  getCurrentUser(): Observable<User> {
    return this.http
      .get<User>(`${this.apiUrl}/me`)
      .pipe(tap((user) => this.currentUserSubject.next(user)));
  }

  updateProfile(data: {
    firstName: string;
    lastName: string;
  }): Observable<User> {
    return this.http.put<User>(`${this.apiUrl}/profile`, data);
  }

  changePassword(data: {
    currentPassword: string;
    newPassword: string;
  }): Observable<{ success: boolean; message: string }> {
    return this.http.put<{ success: boolean; message: string }>(
      `${this.apiUrl}/password`,
      data,
    );
  }

  private handleAuthResponse(response: AuthTokens): void {
    this.tokenService.setTokens(response);
    this.getCurrentUser().subscribe();
  }

  private clearAuth(): void {
    this.tokenService.clearTokens();
    this.currentUserSubject.next(null);
  }

  private loadCurrentUser(): void {
    if (this.tokenService.hasValidToken()) {
      this.getCurrentUser().subscribe({
        error: () => this.clearAuth(),
      });
    }
  }
}
```

### Token Service

```typescript
// token.service.ts
import { Injectable } from "@angular/core";
import { AuthTokens } from "../models/auth-tokens.model";

@Injectable({
  providedIn: "root",
})
export class TokenService {
  private readonly ACCESS_TOKEN_KEY = "access_token";
  private readonly REFRESH_TOKEN_KEY = "refresh_token";
  private readonly TOKEN_EXPIRY_KEY = "token_expiry";

  setTokens(tokens: AuthTokens): void {
    localStorage.setItem(this.ACCESS_TOKEN_KEY, tokens.accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, tokens.refreshToken);
    localStorage.setItem(this.TOKEN_EXPIRY_KEY, tokens.accessTokenExpiresAt);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.ACCESS_TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  getTokenExpiry(): Date | null {
    const expiry = localStorage.getItem(this.TOKEN_EXPIRY_KEY);
    return expiry ? new Date(expiry) : null;
  }

  hasValidToken(): boolean {
    const token = this.getAccessToken();
    const expiry = this.getTokenExpiry();

    if (!token || !expiry) {
      return false;
    }

    // Check if token is expired (with 1 minute buffer)
    return new Date() < new Date(expiry.getTime() - 60000);
  }

  isTokenExpiringSoon(): boolean {
    const expiry = this.getTokenExpiry();
    if (!expiry) return true;

    const now = new Date();
    const fiveMinutesFromNow = new Date(now.getTime() + 5 * 60000);
    return expiry <= fiveMinutesFromNow;
  }

  clearTokens(): void {
    localStorage.removeItem(this.ACCESS_TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.TOKEN_EXPIRY_KEY);
  }
}
```

### Organization Service

```typescript
// organization.service.ts
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { environment } from "@environments/environment";
import {
  Organization,
  OrganizationMember,
} from "@core/models/organization.model";

@Injectable({
  providedIn: "root",
})
export class OrganizationService {
  private apiUrl = `${environment.apiUrl}/organizations`;

  constructor(private http: HttpClient) {}

  getCurrentOrganization(): Observable<Organization> {
    return this.http.get<Organization>(`${this.apiUrl}/current`);
  }

  getOrganization(id: string): Observable<Organization> {
    return this.http.get<Organization>(`${this.apiUrl}/${id}`);
  }

  updateOrganization(
    id: string,
    data: Partial<Organization>,
  ): Observable<Organization> {
    return this.http.put<Organization>(`${this.apiUrl}/${id}`, data);
  }

  getMembers(organizationId: string): Observable<OrganizationMember[]> {
    return this.http.get<OrganizationMember[]>(
      `${this.apiUrl}/${organizationId}/members`,
    );
  }

  inviteMember(
    organizationId: string,
    email: string,
    role: string,
  ): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${organizationId}/invite`, {
      email,
      role,
    });
  }

  removeMember(organizationId: string, userId: string): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/${organizationId}/members/${userId}`,
    );
  }

  updateMemberRole(
    organizationId: string,
    userId: string,
    role: string,
  ): Observable<void> {
    return this.http.put<void>(
      `${this.apiUrl}/${organizationId}/members/${userId}/role`,
      { role },
    );
  }
}
```

---

## 20. Angular Routing

### App Routing Module

```typescript
// app-routing.module.ts
import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { authGuard } from "@core/auth/guards/auth.guard";
import { guestGuard } from "@core/auth/guards/guest.guard";

const routes: Routes = [
  {
    path: "",
    redirectTo: "dashboard",
    pathMatch: "full",
  },
  {
    path: "auth",
    canActivate: [guestGuard],
    loadChildren: () =>
      import("./features/auth/auth.module").then((m) => m.AuthModule),
  },
  {
    path: "dashboard",
    canActivate: [authGuard],
    loadChildren: () =>
      import("./features/dashboard/dashboard.module").then(
        (m) => m.DashboardModule,
      ),
  },
  {
    path: "profile",
    canActivate: [authGuard],
    loadChildren: () =>
      import("./features/profile/profile.module").then((m) => m.ProfileModule),
  },
  {
    path: "organizations",
    canActivate: [authGuard],
    loadChildren: () =>
      import("./features/organizations/organization.module").then(
        (m) => m.OrganizationModule,
      ),
  },
  {
    path: "**",
    redirectTo: "dashboard",
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
```

### Auth Routing Module

```typescript
// auth-routing.module.ts
import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { LoginComponent } from "./login/login.component";
import { RegisterComponent } from "./register/register.component";
import { ForgotPasswordComponent } from "./forgot-password/forgot-password.component";

const routes: Routes = [
  {
    path: "",
    redirectTo: "login",
    pathMatch: "full",
  },
  {
    path: "login",
    component: LoginComponent,
  },
  {
    path: "register",
    component: RegisterComponent,
  },
  {
    path: "forgot-password",
    component: ForgotPasswordComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AuthRoutingModule {}
```

### Auth Guard

```typescript
// auth.guard.ts
import { inject } from "@angular/core";
import { CanActivateFn, Router } from "@angular/router";
import { AuthService } from "@core/auth/services/auth.service";
import { map, take } from "rxjs/operators";

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.isAuthenticated
    ? true
    : router.createUrlTree(["/auth/login"], {
        queryParams: { returnUrl: state.url },
      });
};

export const guestGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.isAuthenticated
    ? router.createUrlTree(["/dashboard"])
    : true;
};
```

---

## 21. UI Wireframes

### Login Page Wireframe

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│                    MONITORING PLATFORM                      │
│                                                             │
│    ┌─────────────────────────────────────────────────┐    │
│    │                                                 │    │
│    │              Welcome Back                       │    │
│    │                                                 │    │
│    │    Email                                        │    │
│    │    ┌─────────────────────────────────────┐    │    │
│    │    │                                     │    │    │
│    │    └─────────────────────────────────────┘    │    │
│    │                                                 │    │
│    │    Password                                     │    │
│    │    ┌─────────────────────────────────────┐    │    │
│    │    │                                     │    │    │
│    │    └─────────────────────────────────────┘    │    │
│    │                                                 │    │
│    │    ☐ Remember me        Forgot password?       │    │
│    │                                                 │    │
│    │    ┌─────────────────────────────────────┐    │    │
│    │    │           SIGN IN                    │    │    │
│    │    └─────────────────────────────────────┘    │    │
│    │                                                 │    │
│    │    ──────────────── or ────────────────       │    │
│    │                                                 │    │
│    │    Don't have an account? Sign up              │    │
│    │                                                 │    │
│    └─────────────────────────────────────────────────┘    │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Register Page Wireframe

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│                    MONITORING PLATFORM                      │
│                                                             │
│    ┌─────────────────────────────────────────────────┐    │
│    │                                                 │    │
│    │              Create Account                     │    │
│    │                                                 │    │
│    │    First Name          Last Name               │    │
│    │    ┌─────────────┐    ┌─────────────┐         │    │
│    │    │             │    │             │         │    │
│    │    └─────────────┘    └─────────────┘         │    │
│    │                                                 │    │
│    │    Email                                        │    │
│    │    ┌─────────────────────────────────────┐    │    │
│    │    │                                     │    │    │
│    │    └─────────────────────────────────────┘    │    │
│    │                                                 │    │
│    │    Password                                     │    │
│    │    ┌─────────────────────────────────────┐    │    │
│    │    │                                     │    │    │
│    │    └─────────────────────────────────────┘    │    │
│    │                                                 │    │
│    │    Confirm Password                             │    │
│    │    ┌─────────────────────────────────────┐    │    │
│    │    │                                     │    │    │
│    │    └─────────────────────────────────────┘    │    │
│    │                                                 │    │
│    │    Organization Name (optional)                 │    │
│    │    ┌─────────────────────────────────────┐    │    │
│    │    │                                     │    │    │
│    │    └─────────────────────────────────────┘    │    │
│    │                                                 │    │
│    │    ┌─────────────────────────────────────┐    │    │
│    │    │         CREATE ACCOUNT               │    │    │
│    │    └─────────────────────────────────────┘    │    │
│    │                                                 │    │
│    │    Already have an account? Sign in            │    │
│    │                                                 │    │
│    └─────────────────────────────────────────────────┘    │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Dashboard Wireframe

```
┌─────────────────────────────────────────────────────────────┐
│  LOGO    Dashboard    Monitors    Alerts    Teams    [User▼]│
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Welcome back, John!                           [Search...]  │
│                                                             │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌───────┐│
│  │  Monitors   │ │   Uptime    │ │  Incidents  │ │ Teams ││
│  │     12      │ │   99.9%     │ │     2       │ │   3   ││
│  └─────────────┘ └─────────────┘ └─────────────┘ └───────┘│
│                                                             │
│  Monitor Status                                            │
│  ┌─────────────────────────────────────────────────────┐  │
│  │  Name              Status    Uptime    Response     │  │
│  ├─────────────────────────────────────────────────────┤  │
│  │  API Server        ● Active   99.9%      120ms      │  │
│  │  Website           ● Active   100%        85ms      │  │
│  │  Database          ● Active   99.5%       45ms      │  │
│  │  Payment API       ○ Down      -          -         │  │
│  └─────────────────────────────────────────────────────┘  │
│                                                             │
│  Recent Incidents                                          │
│  ┌─────────────────────────────────────────────────────┐  │
│  │  Payment API - Down (5 min ago)                     │  │
│  │  Database - Slow response (1 hour ago)              │  │
│  └─────────────────────────────────────────────────────┘  │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Profile Page Wireframe

```
┌─────────────────────────────────────────────────────────────┐
│  LOGO    Dashboard    Monitors    Alerts    Teams    [User▼]│
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Profile Settings                                          │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐  │
│  │  Personal Information                               │  │
│  │                                                     │  │
│  │  Email (read-only)                                  │  │
│  │  ┌─────────────────────────────────────┐           │  │
│  │  │ john@example.com                     │           │  │
│  │  └─────────────────────────────────────┘           │  │
│  │                                                     │  │
│  │  First Name                                         │  │
│  │  ┌─────────────────────────────────────┐           │  │
│  │  │ John                                 │           │  │
│  │  └─────────────────────────────────────┘           │  │
│  │                                                     │  │
│  │  Last Name                                          │  │
│  │  ┌─────────────────────────────────────┐           │  │
│  │  │ Doe                                  │           │  │
│  │  └─────────────────────────────────────┘           │  │
│  │                                                     │  │
│  │  Role: Admin                                        │  │
│  │  Organization: Acme Corp                            │  │
│  │                                                     │  │
│  │  [Save Changes]                                     │  │
│  └─────────────────────────────────────────────────────┘  │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐  │
│  │  Change Password                                    │  │
│  │                                                     │  │
│  │  Current Password                                   │  │
│  │  ┌─────────────────────────────────────┐           │  │
│  │  │ ********                             │           │  │
│  │  └─────────────────────────────────────┘           │  │
│  │                                                     │  │
│  │  New Password                                       │  │
│  │  ┌─────────────────────────────────────┐           │  │
│  │  │ ********                             │           │  │
│  │  └─────────────────────────────────────┘           │  │
│  │                                                     │  │
│  │  [Change Password]                                  │  │
│  └─────────────────────────────────────────────────────┘  │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 22. Unit Tests

### Backend Unit Tests

#### RegisterCommandHandler Tests

```csharp
namespace MonitoringPlatform.Application.Tests.Handlers;

public class RegisterCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _organizationRepository = Substitute.For<IOrganizationRepository>();
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        _jwtTokenService = Substitute.For<IJwtTokenService>();
        _passwordHashingService = Substitute.For<IPasswordHashingService>();
        _emailService = Substitute.For<IEmailService>();
        _mapper = Substitute.For<IMapper>();

        _handler = new RegisterCommandHandler(
            _userRepository,
            _organizationRepository,
            _refreshTokenRepository,
            _jwtTokenService,
            _passwordHashingService,
            _emailService,
            _mapper,
            Substitute.For<ILogger<RegisterCommandHandler>>()
        );
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsDuplicateException()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        _userRepository.GetByEmailAsync(command.Email).Returns(new User());

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsRegisterResponse()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "new@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        _userRepository.GetByEmailAsync(command.Email).Returns((User?)null);
        _organizationRepository.CreateAsync(Arg.Any<Organization>()).Returns(new Organization { OrganizationId = Guid.NewGuid(), Name = "Test Org" });
        _userRepository.CreateAsync(Arg.Any<User>()).Returns(new User { UserId = Guid.NewGuid() });
        _jwtTokenService.GenerateTokensAsync(Arg.Any<User>()).Returns(new AuthTokens("token", "refresh", DateTime.UtcNow.AddMinutes(15), DateTime.UtcNow.AddDays(7)));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(command.Email);
        result.FirstName.Should().Be(command.FirstName);
        result.LastName.Should().Be(command.LastName);
    }
}
```

#### LoginCommandHandler Tests

```csharp
namespace MonitoringPlatform.Application.Tests.Handlers;

public class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        _jwtTokenService = Substitute.For<IJwtTokenService>();
        _passwordHashingService = Substitute.For<IPasswordHashingService>();

        _handler = new LoginCommandHandler(
            _userRepository,
            _refreshTokenRepository,
            _jwtTokenService,
            _passwordHashingService,
            Substitute.For<IMapper>(),
            Substitute.For<ILogger<LoginCommandHandler>>()
        );
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsUnauthorizedException()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "notfound@example.com",
            Password = "Password123!"
        };

        _userRepository.GetByEmailAsync(command.Email).Returns((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InvalidPassword_ThrowsUnauthorizedException()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "WrongPassword123!"
        };

        var user = new User { UserId = Guid.NewGuid(), Email = command.Email, PasswordHash = "hashedpassword", Status = UserStatus.Active };
        _userRepository.GetByEmailAsync(command.Email).Returns(user);
        _passwordHashingService.VerifyPassword(command.Password, user.PasswordHash).Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = command.Email,
            PasswordHash = "hashedpassword",
            Status = UserStatus.Active,
            OrganizationId = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Role = UserRole.Owner,
            Organization = new Organization { Name = "Test Org" }
        };

        _userRepository.GetByEmailAsync(command.Email).Returns(user);
        _passwordHashingService.VerifyPassword(command.Password, user.PasswordHash).Returns(true);
        _jwtTokenService.GenerateTokensAsync(user).Returns(new AuthTokens("token", "refresh", DateTime.UtcNow.AddMinutes(15), DateTime.UtcNow.AddDays(7)));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(user.UserId);
        result.Email.Should().Be(user.Email);
        result.Tokens.Should().NotBeNull();
    }
}
```

#### Validator Tests

```csharp
namespace MonitoringPlatform.Application.Tests.Validators;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator;

    public RegisterCommandValidatorTests()
    {
        _validator = new RegisterCommandValidator();
    }

    [Theory]
    [InlineData("", "Password123!", "John", "Doe")]
    [InlineData("invalid-email", "Password123!", "John", "Doe")]
    [InlineData("test@example.com", "short", "John", "Doe")]
    [InlineData("test@example.com", "Password123!", "", "Doe")]
    public void Validate_InvalidData_HasErrors(string email, string password, string firstName, string lastName)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = email,
            Password = password,
            FirstName = firstName,
            LastName = lastName
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void Validate_ValidData_NoErrors()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "valid@example.com",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
```

### Frontend Unit Tests

#### Auth Service Tests

```typescript
// auth.service.spec.ts
import { TestBed } from "@angular/core/testing";
import {
  HttpClientTestingModule,
  HttpTestingController,
} from "@angular/common/http/testing";
import { AuthService } from "./auth.service";
import { TokenService } from "./token.service";
import { environment } from "@environments/environment";

describe("AuthService", () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let tokenService: jasmine.SpyObj<TokenService>;

  beforeEach(() => {
    const tokenServiceSpy = jasmine.createSpyObj("TokenService", [
      "setTokens",
      "getAccessToken",
      "getRefreshToken",
      "getTokenExpiry",
      "hasValidToken",
      "isTokenExpiringSoon",
      "clearTokens",
    ]);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        AuthService,
        { provide: TokenService, useValue: tokenServiceSpy },
      ],
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    tokenService = TestBed.inject(TokenService) as jasmine.SpyObj<TokenService>;
  });

  afterEach(() => {
    httpMock.verify();
  });

  it("should be created", () => {
    expect(service).toBeTruthy();
  });

  describe("login", () => {
    it("should call login endpoint and handle response", (done) => {
      // Arrange
      const loginRequest = {
        email: "test@example.com",
        password: "password123",
      };
      const mockResponse = {
        accessToken: "token",
        refreshToken: "refresh",
        accessTokenExpiresAt: new Date().toISOString(),
        refreshTokenExpiresAt: new Date().toISOString(),
      };

      tokenService.hasValidToken.and.returnValue(false);

      // Act
      service.login(loginRequest).subscribe((response) => {
        expect(response).toEqual(mockResponse);
        expect(tokenService.setTokens).toHaveBeenCalledWith(mockResponse);
        done();
      });

      // Assert
      const req = httpMock.expectOne(`${environment.apiUrl}/auth/login`);
      expect(req.request.method).toBe("POST");
      req.flush(mockResponse);
    });
  });

  describe("register", () => {
    it("should call register endpoint and handle response", (done) => {
      // Arrange
      const registerRequest = {
        email: "new@example.com",
        password: "SecurePass123!",
        firstName: "John",
        lastName: "Doe",
      };
      const mockResponse = {
        accessToken: "token",
        refreshToken: "refresh",
        accessTokenExpiresAt: new Date().toISOString(),
        refreshTokenExpiresAt: new Date().toISOString(),
      };

      tokenService.hasValidToken.and.returnValue(false);

      // Act
      service.register(registerRequest).subscribe((response) => {
        expect(response).toEqual(mockResponse);
        expect(tokenService.setTokens).toHaveBeenCalledWith(mockResponse);
        done();
      });

      // Assert
      const req = httpMock.expectOne(`${environment.apiUrl}/auth/register`);
      expect(req.request.method).toBe("POST");
      req.flush(mockResponse);
    });
  });

  describe("logout", () => {
    it("should call logout endpoint and clear tokens", (done) => {
      // Act
      service.logout().subscribe(() => {
        expect(tokenService.clearTokens).toHaveBeenCalled();
        done();
      });

      // Assert
      const req = httpMock.expectOne(`${environment.apiUrl}/auth/logout`);
      expect(req.request.method).toBe("POST");
      req.flush({});
    });
  });
});
```

---

## 23. Integration Tests

### API Integration Tests

```csharp
namespace MonitoringPlatform.API.Tests.Integration;

public class AuthenticationEndpointTests : IClassFixture<IntegrationTestFactory>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestFactory _factory;

    public AuthenticationEndpointTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsCreatedWithTokens()
    {
        // Arrange
        var request = new
        {
            email = $"test{Guid.NewGuid()}@example.com",
            password = "SecurePass123!",
            firstName = "John",
            lastName = "Doe",
            organizationName = "Test Org"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadFromJsonAsync<RegisterResponseDto>();
        content.Should().NotBeNull();
        content!.Email.Should().Be(request.email);
        content.FirstName.Should().Be(request.firstName);
        content.LastName.Should().Be(request.lastName);
        content.Tokens.Should().NotBeNull();
        content.Tokens.AccessToken.Should().NotBeNullOrEmpty();
        content.Tokens.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var request = new
        {
            email = "existing@example.com",
            password = "SecurePass123!",
            firstName = "John",
            lastName = "Doe"
        };

        // First registration
        await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Act - Second registration with same email
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokens()
    {
        // Arrange
        var registerRequest = new
        {
            email = $"login{Guid.NewGuid()}@example.com",
            password = "SecurePass123!",
            firstName = "John",
            lastName = "Doe"
        };

        await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        var loginRequest = new
        {
            email = registerRequest.email,
            password = registerRequest.password
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        content.Should().NotBeNull();
        content!.Tokens.Should().NotBeNull();
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new
        {
            email = "notexist@example.com",
            password = "WrongPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_ValidToken_ReturnsNewTokens()
    {
        // Arrange - Register and login to get refresh token
        var registerRequest = new
        {
            email = $"refresh{Guid.NewGuid()}@example.com",
            password = "SecurePass123!",
            firstName = "John",
            lastName = "Doe"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        var loginContent = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();

        var refreshRequest = new
        {
            refreshToken = loginContent!.Tokens.RefreshToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<RefreshTokenResponseDto>();
        content.Should().NotBeNull();
        content!.Tokens.Should().NotBeNull();
        content.Tokens.AccessToken.Should().NotBe(loginContent.Tokens.AccessToken);
    }

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ReturnsUserProfile()
    {
        // Arrange - Register and login
        var registerRequest = new
        {
            email = $"profile{Guid.NewGuid()}@example.com",
            password = "SecurePass123!",
            firstName = "John",
            lastName = "Doe"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        var loginContent = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginContent!.Tokens.AccessToken);

        // Act
        var response = await _client.GetAsync("/api/v1/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<UserProfileResponseDto>();
        content.Should().NotBeNull();
        content!.Email.Should().Be(registerRequest.email);
        content.FirstName.Should().Be(registerRequest.firstName);
        content.LastName.Should().Be(registerRequest.lastName);
    }

    [Fact]
    public async Task GetCurrentUser_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var response = await _client.GetAsync("/api/v1/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class IntegrationTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove real database context
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });

            // Build service provider
            var sp = services.BuildServiceProvider();

            // Create scope and get context
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();

            // Ensure database is created
            db.Database.EnsureCreated();
        });
    }
}
```

---

## 24. Security Considerations

### Authentication Security

1. **Password Hashing**
   - Use BCrypt with work factor of 12
   - Never store plain text passwords
   - Password must meet complexity requirements

2. **JWT Token Security**
   - Access tokens expire in 15 minutes
   - Refresh tokens expire in 7 days
   - Tokens signed with HS256 algorithm
   - Secret key minimum 256 bits
   - Store tokens in memory (not localStorage for production)

3. **Refresh Token Rotation**
   - Old token is revoked when new one is issued
   - Detect token reuse attacks
   - Family of tokens tracking

4. **Rate Limiting**
   - Login: 5 attempts per minute per IP
   - Register: 3 attempts per minute per IP
   - Refresh: 10 attempts per minute per IP

5. **Account Lockout**
   - Lock account after 5 failed login attempts
   - 15-minute lockout duration
   - Notify user via email

### Data Protection

1. **Input Validation**
   - All inputs validated server-side
   - SQL injection prevention via EF Core
   - XSS prevention via output encoding

2. **HTTPS Enforcement**
   - All API endpoints require HTTPS
   - HSTS header enabled
   - TLS 1.3 preferred

3. **CORS Configuration**
   - Whitelist allowed origins
   - Credentials allowed only for trusted origins
   - Preflight caching

4. **Audit Logging**
   - Log all authentication events
   - Log failed attempts with IP
   - Log token refresh events
   - Retain logs for 90 days

### OWASP Compliance

1. **A01: Broken Access Control**
   - Role-based authorization on all endpoints
   - Resource-level permission checks
   - Principle of least privilege

2. **A02: Cryptographic Failures**
   - TLS 1.3 for data in transit
   - BCrypt for password hashing
   - SHA-256 for token hashing

3. **A03: Injection**
   - Parameterized queries via EF Core
   - Input validation and sanitization
   - No dynamic SQL

4. **A04: Insecure Design**
   - Threat modeling for auth flows
   - Security by default
   - Fail securely

5. **A05: Security Misconfiguration**
   - Secure default configurations
   - No debug info in production
   - Security headers configured

6. **A06: Vulnerable Components**
   - Regular dependency updates
   - SCA scanning in CI/CD
   - CVE monitoring

7. **A07: Authentication Failures**
   - MFA support ready
   - Session management
   - Brute force protection

8. **A08: Software & Data Integrity**
   - Digital signatures for tokens
   - Integrity checks on downloads
   - CI/CD pipeline security

9. **A09: Security Logging**
   - Comprehensive logging
   - Log monitoring
   - Alert on suspicious activity

10. **A10: SSRF**
    - Input validation for URLs
    - No raw URL fetching
    - Allowlist for external calls

---

## 25. Deliverables

### Code Deliverables

- [ ] **Backend API**
  - [ ] Clean Architecture solution with 4 projects
  - [ ] Complete authentication endpoints
  - [ ] User management endpoints
  - [ ] Organization management endpoints
  - [ ] Team management endpoints
  - [ ] EF Core with migrations
  - [ ] JWT authentication implementation
  - [ ] Refresh token mechanism
  - [ ] Password hashing service
  - [ ] Input validation with FluentValidation

- [ ] **Frontend Application**
  - [ ] Angular application with standalone components
  - [ ] Login page
  - [ ] Register page
  - [ ] Dashboard page
  - [ ] Profile page
  - [ ] Auth service with token management
  - [ ] HTTP interceptors for JWT
  - [ ] Route guards
  - [ ] Responsive design

- [ ] **Database**
  - [ ] PostgreSQL schema
  - [ ] EF Core migrations
  - [ ] Seed data for testing
  - [ ] Indexes for performance

### Documentation Deliverables

- [ ] **API Documentation**
  - [ ] Swagger/OpenAPI specification
  - [ ] Endpoint documentation
  - [ ] Request/response examples
  - [ ] Error code documentation

- [ ] **Technical Documentation**
  - [ ] Architecture decision records
  - [ ] Database schema documentation
  - [ ] Authentication flow diagrams
  - [ ] Deployment guide

### Testing Deliverables

- [ ] **Unit Tests**
  - [ ] Backend unit tests (>80% coverage)
  - [ ] Frontend unit tests
  - [ ] Validator tests
  - [ ] Handler tests

- [ ] **Integration Tests**
  - [ ] API endpoint tests
  - [ ] Authentication flow tests
  - [ ] Database integration tests

### DevOps Deliverables

- [ ] **CI/CD Pipeline**
  - [ ] GitHub Actions workflow
  - [ ] Build and test pipeline
  - [ ] Docker image building
  - [ ] Deployment automation

- [ ] **Infrastructure**
  - [ ] Dockerfile for API
  - [ ] Docker Compose for local development
  - [ ] Environment configuration templates

### Security Deliverables

- [ ] **Security Implementation**
  - [ ] JWT authentication
  - [ ] Password hashing
  - [ ] Rate limiting
  - [ ] CORS configuration
  - [ ] Security headers

- [ ] **Security Documentation**
  - [ ] Security considerations document
  - [ ] OWASP compliance checklist
  - [ ] Threat model

---

## Sprint Timeline

| Day       | Tasks                                                          |
| --------- | -------------------------------------------------------------- |
| Day 1-2   | Project setup, Clean Architecture scaffolding, Database design |
| Day 3-4   | Entity definitions, EF Core configurations, Migrations         |
| Day 5-6   | Authentication commands/queries, Handlers implementation       |
| Day 7-8   | JWT service, Password hashing, Email service                   |
| Day 9-10  | API endpoints, Controllers, Middleware                         |
| Day 11-12 | Angular project setup, Auth module, Services                   |
| Day 13-14 | Login/Register pages, Routing, Guards                          |
| Day 15-16 | Profile page, Organization service                             |
| Day 17-18 | Unit tests, Integration tests                                  |
| Day 19-20 | Documentation, CI/CD setup, Sprint review                      |

---

## Definition of Done

A user story is considered complete when:

1. ✅ Code is implemented and follows Clean Architecture principles
2. ✅ All acceptance criteria are met
3. ✅ Unit tests pass with >80% coverage
4. ✅ Integration tests pass
5. ✅ Code is reviewed and approved
6. ✅ Documentation is updated
7. ✅ No security vulnerabilities detected
8. ✅ Performance benchmarks are met
9. ✅ Feature is deployed to staging environment
10. ✅ Product owner has accepted the feature
