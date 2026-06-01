# Configuration Audit Report - Monitoring Platform

## Executive Summary

This report identifies all hard-coded configuration values found in the Monitoring Platform solution and provides a refactoring plan to move them to strongly typed configuration classes using the ASP.NET Core Options Pattern.

---

## 🔍 DISCOVERED HARD-CODED VALUES

### 1. Email/SMTP Configuration
**Location:** `src/MonitoringPlatform.Infrastructure/Services/EmailService.cs`

**Hard-coded values:**
```csharp
private readonly string _smtpHost = "smtp.mailtrap.io";
private readonly int _smtpPort = 2525;
private readonly string _smtpUser = "your_mailtrap_username";
private readonly string _smtpPass = "your_mailtrap_password";
private readonly string _fromEmail = "no-reply@monitoringplatform.com";
// EnableSsl = true (hard-coded in method)
```

**Security Risk:** ⚠️ HIGH - Credentials in source code

---

### 2. JWT Configuration
**Location:** `src/MonitoringPlatform.Infrastructure/Security/JwtService.cs`

**Status:** ✅ ALREADY USING OPTIONS PATTERN

**Current implementation:**
```csharp
public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string ValidIssuer { get; set; } = string.Empty;
    public string ValidAudience { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
```

**Configuration in appsettings.json:**
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!ChangeThisInProduction",
    "ValidIssuer": "MonitoringPlatform.API",
    "ValidAudience": "MonitoringPlatform.Client",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

**Security Risk:** ⚠️ HIGH - Secret key in appsettings.json (should be in User Secrets/Key Vault)

**Improvement needed:** Add validation and move to secure storage

---

### 3. Database Connection Strings
**Location:** `src/MonitoringPlatform.API/appsettings.json` and `appsettings.Development.json`

**Hard-coded values:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=monitoring_platform;Username=postgres;Password=postgres"
}
```

**Security Risk:** ⚠️ HIGH - Database credentials in appsettings.json

**Status:** ✅ Using standard ConnectionStrings pattern, but needs secure storage

---

### 4. Background Worker Configuration
**Location:** `src/MonitoringPlatform.Infrastructure/BackgroundServices/NotificationWorker.cs`

**Hard-coded values:**
```csharp
// Line 42: Maximum retry attempts
if (alertEvent.AttemptCount >= 3)

// Line 74: Polling interval
await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
```

**Configuration needed:**
- MaxRetryAttempts: 3
- PollingIntervalSeconds: 30

**Security Risk:** ⚠️ LOW - Operational configuration

---

### 5. JWT Authentication Configuration
**Location:** `src/MonitoringPlatform.Infrastructure/DependencyInjection.cs`

**Hard-coded values:**
```csharp
// Line 68
ClockSkew = TimeSpan.Zero
```

**Configuration needed:**
- ClockSkew tolerance

**Security Risk:** ⚠️ LOW - Security configuration

---

### 6. Seed Data Configuration
**Location:** `src/MonitoringPlatform.API/Data/DbSeed.cs`

**Hard-coded values:**
```csharp
// Line 39
Email = "admin@example.com"

// Line 123
Target = "https://example.com"
```

**Configuration needed:**
- Default admin email
- Sample monitor URLs

**Security Risk:** ⚠️ MEDIUM - Default credentials

---

### 7. CORS Configuration
**Location:** `src/MonitoringPlatform.API/Program.cs`

**Hard-coded values:**
```csharp
options.AddPolicy("AllowAll", policy =>
{
    policy.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader();
});
```

**Security Risk:** ⚠️ HIGH - Allows all origins (not suitable for production)

**Configuration needed:**
- Allowed origins list
- Allowed methods
- Allowed headers

---

## 📊 SUMMARY STATISTICS

| Category | Count | Security Risk |
|----------|-------|---------------|
| Email/SMTP Settings | 6 values | HIGH |
| JWT Settings | 5 values | HIGH (needs secure storage) |
| Database Settings | 1 value | HIGH (needs secure storage) |
| Background Worker | 2 values | LOW |
| Authentication | 1 value | LOW |
| Seed Data | 2 values | MEDIUM |
| CORS | 3 values | HIGH |
| **TOTAL** | **20 values** | **5 HIGH, 1 MEDIUM, 3 LOW** |

---

## 🎯 REFACTORING PLAN

### Phase 1: Create Settings Classes
1. ✅ JwtSettings (already exists)
2. 🔨 EmailSettings
3. 🔨 NotificationWorkerSettings
4. 🔨 CorsSettings
5. 🔨 SeedDataSettings

### Phase 2: Update Services
1. 🔨 EmailService - Use IOptions<EmailSettings>
2. 🔨 NotificationWorker - Use IOptions<NotificationWorkerSettings>
3. 🔨 Program.cs - Use CorsSettings

### Phase 3: Update Configuration Files
1. 🔨 appsettings.json - Add new sections
2. 🔨 appsettings.Development.json - Add development overrides

### Phase 4: Add Validation
1. 🔨 Data annotations on settings classes
2. 🔨 ValidateDataAnnotations() in DI registration
3. 🔨 ValidateOnStart() for startup validation

### Phase 5: Security Improvements
1. 🔨 Move secrets to User Secrets (development)
2. 🔨 Document Azure Key Vault integration (production)
3. 🔨 Add environment variable support

---

## 🔐 SECURITY RECOMMENDATIONS

### Immediate Actions:
1. **Remove hard-coded SMTP credentials** from EmailService.cs
2. **Move JWT SecretKey** to User Secrets (development) and Key Vault (production)
3. **Move database passwords** to User Secrets/Key Vault
4. **Restrict CORS policy** to specific origins

### Best Practices:
1. Never commit secrets to source control
2. Use User Secrets for local development
3. Use Azure Key Vault or similar for production
4. Use environment variables for container deployments
5. Implement configuration validation at startup
6. Use different settings per environment

---

## 📝 IMPLEMENTATION CHECKLIST

- [ ] Create EmailSettings class
- [ ] Create NotificationWorkerSettings class
- [ ] Create CorsSettings class
- [ ] Create SeedDataSettings class
- [ ] Refactor EmailService to use Options Pattern
- [ ] Refactor NotificationWorker to use Options Pattern
- [ ] Update appsettings.json with new sections
- [ ] Add configuration validation
- [ ] Update DI registration in Program.cs
- [ ] Move secrets to User Secrets
- [ ] Document Key Vault integration
- [ ] Update README with configuration instructions
- [ ] Test all configurations

---

## 🚀 EXPECTED BENEFITS

1. **Security:** Secrets removed from source code
2. **Flexibility:** Easy to change configuration per environment
3. **Maintainability:** Centralized configuration management
4. **Validation:** Startup validation prevents runtime errors
5. **Testability:** Easy to mock configuration in tests
6. **Documentation:** Strongly typed settings serve as documentation

---

## 📖 NEXT STEPS

1. Review and approve this audit report
2. Begin Phase 1: Create settings classes
3. Implement Phase 2: Refactor services
4. Update Phase 3: Configuration files
5. Add Phase 4: Validation
6. Implement Phase 5: Security improvements
7. Test thoroughly in all environments
8. Update documentation

---

**Report Generated:** 2026-06-01  
**Solution:** Monitoring Platform  
**Architecture:** Clean Architecture with CQRS  
**Framework:** ASP.NET Core 9.0
