# Security Configuration Guide - Monitoring Platform

## 🔐 Moving Secrets to Secure Storage

This guide provides step-by-step instructions for securing sensitive configuration values in the Monitoring Platform.

---

## ⚠️ CRITICAL: Secrets Currently in Source Code

The following secrets are currently stored in `appsettings.json` and **MUST** be moved to secure storage:

| Secret | Current Location | Risk Level |
|--------|-----------------|------------|
| JWT SecretKey | `appsettings.json` | 🔴 CRITICAL |
| SMTP Username | `appsettings.json` | 🔴 HIGH |
| SMTP Password | `appsettings.json` | 🔴 HIGH |
| Database Password | `appsettings.json` | 🔴 CRITICAL |
| Default Admin Password | `appsettings.json` | 🟡 MEDIUM |

---

## 🛠️ STEP 1: User Secrets (Development Environment)

User Secrets is a secure way to store secrets during local development. Secrets are stored outside the project directory and never committed to source control.

### Initialize User Secrets

```bash
cd src/MonitoringPlatform.API
dotnet user-secrets init
```

This adds a `UserSecretsId` to your `.csproj` file:

```xml
<PropertyGroup>
  <UserSecretsId>aspnet-MonitoringPlatform-12345678</UserSecretsId>
</PropertyGroup>
```

### Set Secrets

```bash
# JWT Settings
dotnet user-secrets set "JwtSettings:SecretKey" "YourActualSecretKeyHere_MustBeAtLeast32CharactersLong!"

# Email Settings
dotnet user-secrets set "EmailSettings:Username" "your_actual_smtp_username"
dotnet user-secrets set "EmailSettings:Password" "your_actual_smtp_password"

# Database Connection String
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=monitoring_platform_dev;Username=postgres;Password=your_actual_db_password"

# Seed Data Settings
dotnet user-secrets set "SeedDataSettings:DefaultAdminPassword" "YourSecureAdminPassword123!"
```

### List All Secrets

```bash
dotnet user-secrets list
```

### Remove a Secret

```bash
dotnet user-secrets remove "EmailSettings:Password"
```

### Clear All Secrets

```bash
dotnet user-secrets clear
```

### Where Are Secrets Stored?

**Windows:**
```
%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json
```

**Linux/macOS:**
```
~/.microsoft/usersecrets/<UserSecretsId>/secrets.json
```

---

## 🌍 STEP 2: Environment Variables (All Environments)

Environment variables work across all environments and are the standard way to configure containerized applications.

### Setting Environment Variables

**Windows (PowerShell):**
```powershell
$env:JwtSettings__SecretKey = "YourSecretKey"
$env:EmailSettings__Username = "smtp_username"
$env:EmailSettings__Password = "smtp_password"
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=monitoring_platform;Username=postgres;Password=secure_password"
```

**Windows (Command Prompt):**
```cmd
set JwtSettings__SecretKey=YourSecretKey
set EmailSettings__Username=smtp_username
set EmailSettings__Password=smtp_password
```

**Linux/macOS:**
```bash
export JwtSettings__SecretKey="YourSecretKey"
export EmailSettings__Username="smtp_username"
export EmailSettings__Password="smtp_password"
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=monitoring_platform;Username=postgres;Password=secure_password"
```

### Docker Compose

```yaml
version: '3.8'
services:
  api:
    image: monitoring-platform-api
    environment:
      - JwtSettings__SecretKey=${JWT_SECRET_KEY}
      - EmailSettings__Username=${SMTP_USERNAME}
      - EmailSettings__Password=${SMTP_PASSWORD}
      - ConnectionStrings__DefaultConnection=${DATABASE_CONNECTION}
    env_file:
      - .env
```

**.env file (DO NOT COMMIT):**
```env
JWT_SECRET_KEY=YourSecretKeyHere
SMTP_USERNAME=your_smtp_username
SMTP_PASSWORD=your_smtp_password
DATABASE_CONNECTION=Host=db;Port=5432;Database=monitoring_platform;Username=postgres;Password=secure_password
```

**Add to .gitignore:**
```
.env
.env.local
.env.production
secrets.json
```

---

## ☁️ STEP 3: Azure Key Vault (Production - Recommended)

Azure Key Vault is the recommended solution for production environments.

### Install NuGet Package

```bash
dotnet add package Azure.Extensions.AspNetCore.Configuration.Secrets
dotnet add package Azure.Identity
```

### Update Program.cs

```csharp
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

var builder = WebApplication.CreateBuilder(args);

// Add Azure Key Vault
if (builder.Environment.IsProduction())
{
    var keyVaultName = builder.Configuration["KeyVaultName"];
    var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
    
    builder.Configuration.AddAzureKeyVault(
        keyVaultUri,
        new DefaultAzureCredential());
}

// Rest of your configuration...
```

### Create Secrets in Azure Key Vault

```bash
# Using Azure CLI
az keyvault secret set --vault-name "your-keyvault-name" \
  --name "JwtSettings--SecretKey" \
  --value "YourSecretKeyHere"

az keyvault secret set --vault-name "your-keyvault-name" \
  --name "EmailSettings--Username" \
  --value "smtp_username"

az keyvault secret set --vault-name "your-keyvault-name" \
  --name "EmailSettings--Password" \
  --value "smtp_password"

az keyvault secret set --vault-name "your-keyvault-name" \
  --name "ConnectionStrings--DefaultConnection" \
  --value "Host=prod-db;Port=5432;Database=monitoring_platform;Username=postgres;Password=secure_password"
```

### Grant Access to Your Application

```bash
# Using Managed Identity (Recommended)
az keyvault set-policy --name "your-keyvault-name" \
  --object-id <your-app-managed-identity-object-id> \
  --secret-permissions get list

# Or using Service Principal
az keyvault set-policy --name "your-keyvault-name" \
  --spn <your-app-service-principal-id> \
  --secret-permissions get list
```

### Configuration in appsettings.Production.json

```json
{
  "KeyVaultName": "your-keyvault-name"
}
```

---

## 🔧 STEP 4: Update appsettings.json

After moving secrets to secure storage, update `appsettings.json` to remove sensitive values:

### Before (❌ INSECURE):
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!ChangeThisInProduction"
  },
  "EmailSettings": {
    "Username": "your_mailtrap_username",
    "Password": "your_mailtrap_password"
  }
}
```

### After (✅ SECURE):
```json
{
  "JwtSettings": {
    "SecretKey": "",
    "ValidIssuer": "MonitoringPlatform.API",
    "ValidAudience": "MonitoringPlatform.Client",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "EmailSettings": {
    "SmtpHost": "smtp.mailtrap.io",
    "SmtpPort": 2525,
    "Username": "",
    "Password": "",
    "FromEmail": "no-reply@monitoringplatform.com",
    "EnableSsl": true,
    "TimeoutSeconds": 30
  }
}
```

**Note:** Empty strings for secrets will be overridden by User Secrets, Environment Variables, or Key Vault.

---

## 🚀 STEP 5: CI/CD Pipeline Configuration

### GitHub Actions

```yaml
name: Deploy to Production

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      
      - name: Build
        run: dotnet build --configuration Release
        env:
          JwtSettings__SecretKey: ${{ secrets.JWT_SECRET_KEY }}
          EmailSettings__Username: ${{ secrets.SMTP_USERNAME }}
          EmailSettings__Password: ${{ secrets.SMTP_PASSWORD }}
      
      - name: Deploy
        run: |
          # Your deployment commands here
```

**Add secrets in GitHub:**
1. Go to repository Settings → Secrets and variables → Actions
2. Add secrets:
   - `JWT_SECRET_KEY`
   - `SMTP_USERNAME`
   - `SMTP_PASSWORD`
   - `DATABASE_CONNECTION`

### Azure DevOps

```yaml
trigger:
  - main

pool:
  vmImage: 'ubuntu-latest'

variables:
  - group: 'MonitoringPlatform-Production'

steps:
  - task: DotNetCoreCLI@2
    displayName: 'Build'
    inputs:
      command: 'build'
      configuration: 'Release'
    env:
      JwtSettings__SecretKey: $(JWT_SECRET_KEY)
      EmailSettings__Username: $(SMTP_USERNAME)
      EmailSettings__Password: $(SMTP_PASSWORD)
```

**Add variable group:**
1. Go to Pipelines → Library → Variable groups
2. Create group "MonitoringPlatform-Production"
3. Add variables and mark as secret

---

## ✅ VERIFICATION CHECKLIST

After implementing secure configuration:

- [ ] User Secrets initialized for development
- [ ] All secrets removed from `appsettings.json`
- [ ] `.env` files added to `.gitignore`
- [ ] Environment variables configured for staging/production
- [ ] Azure Key Vault configured (production)
- [ ] CI/CD pipeline secrets configured
- [ ] Application starts successfully with secure configuration
- [ ] All features work with secure configuration
- [ ] No secrets committed to source control
- [ ] Team members documented on secret management

---

## 🔍 TESTING SECURE CONFIGURATION

### Test User Secrets (Development)

```bash
# Set a test secret
dotnet user-secrets set "EmailSettings:Username" "test_user"

# Run the application
dotnet run --project src/MonitoringPlatform.API

# Verify the secret is loaded (check logs or debug)
```

### Test Environment Variables

```bash
# Set environment variable
export EmailSettings__Username="test_user"

# Run the application
dotnet run --project src/MonitoringPlatform.API

# Verify the variable is loaded
```

### Test Configuration Priority

Configuration sources are loaded in this order (later sources override earlier ones):

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. User Secrets (Development only)
4. Environment Variables
5. Command-line arguments
6. Azure Key Vault (if configured)

---

## 🛡️ SECURITY BEST PRACTICES

### DO ✅

- Use User Secrets for local development
- Use Azure Key Vault for production
- Use environment variables for containers
- Rotate secrets regularly
- Use different secrets per environment
- Limit access to secrets (principle of least privilege)
- Audit secret access
- Use Managed Identity when possible

### DON'T ❌

- Commit secrets to source control
- Share secrets via email or chat
- Use the same secrets across environments
- Store secrets in plain text files
- Log secrets
- Display secrets in error messages
- Hard-code secrets in source code

---

## 🆘 EMERGENCY: Secret Leaked

If a secret is accidentally committed to source control:

1. **Immediately rotate the secret**
   - Generate a new JWT secret key
   - Change SMTP password
   - Change database password

2. **Update all environments**
   - Update User Secrets
   - Update Environment Variables
   - Update Azure Key Vault
   - Update CI/CD secrets

3. **Remove from Git history**
   ```bash
   # Use BFG Repo-Cleaner or git-filter-repo
   git filter-repo --path appsettings.json --invert-paths
   ```

4. **Force push (if safe)**
   ```bash
   git push --force
   ```

5. **Notify team members**
   - Inform all developers
   - Update documentation
   - Review security practices

---

## 📚 ADDITIONAL RESOURCES

- [Safe Storage of App Secrets in Development](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Azure Key Vault Configuration Provider](https://learn.microsoft.com/en-us/aspnet/core/security/key-vault-configuration)
- [Configuration in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Environment Variables in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/#environment-variables)

---

**Document Version:** 1.0  
**Last Updated:** 2026-06-01  
**Status:** ✅ Ready for Implementation
