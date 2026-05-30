# CLI Commands to Create Solution

## Create Solution and Projects

```bash
# Create solution
dotnet new sln -n MonitoringPlatform

# Create API project
dotnet new webapi -n MonitoringPlatform.API -o src/MonitoringPlatform.API

# Create class libraries
dotnet new classlib -n MonitoringPlatform.Application -o src/MonitoringPlatform.Application
dotnet new classlib -n MonitoringPlatform.Domain -o src/MonitoringPlatform.Domain
dotnet new classlib -n MonitoringPlatform.Infrastructure -o src/MonitoringPlatform.Infrastructure

# Create test projects
dotnet new xunit -n MonitoringPlatform.UnitTests -o tests/MonitoringPlatform.UnitTests
dotnet new xunit -n MonitoringPlatform.IntegrationTests -o tests/MonitoringPlatform.IntegrationTests

# Add projects to solution
dotnet sln add src/MonitoringPlatform.API/MonitoringPlatform.API.csproj
dotnet sln add src/MonitoringPlatform.Application/MonitoringPlatform.Application.csproj
dotnet sln add src/MonitoringPlatform.Domain/MonitoringPlatform.Domain.csproj
dotnet sln add src/MonitoringPlatform.Infrastructure/MonitoringPlatform.Infrastructure.csproj
dotnet sln add tests/MonitoringPlatform.UnitTests/MonitoringPlatform.UnitTests.csproj
dotnet sln add tests/MonitoringPlatform.IntegrationTests/MonitoringPlatform.IntegrationTests.csproj

# Add project references
dotnet add src/MonitoringPlatform.API/MonitoringPlatform.API.csproj reference src/MonitoringPlatform.Application/MonitoringPlatform.Application.csproj
dotnet add src/MonitoringPlatform.API/MonitoringPlatform.API.csproj reference src/MonitoringPlatform.Infrastructure/MonitoringPlatform.Infrastructure.csproj

dotnet add src/MonitoringPlatform.Application/MonitoringPlatform.Application.csproj reference src/MonitoringPlatform.Domain/MonitoringPlatform.Domain.csproj

dotnet add src/MonitoringPlatform.Infrastructure/MonitoringPlatform.Infrastructure.csproj reference src/MonitoringPlatform.Application/MonitoringPlatform.Application.csproj
dotnet add src/MonitoringPlatform.Infrastructure/MonitoringPlatform.Infrastructure.csproj reference src/MonitoringPlatform.Domain/MonitoringPlatform.Domain.csproj

dotnet add tests/MonitoringPlatform.UnitTests/MonitoringPlatform.UnitTests.csproj reference src/MonitoringPlatform.Application/MonitoringPlatform.Application.csproj
dotnet add tests/MonitoringPlatform.UnitTests/MonitoringPlatform.UnitTests.csproj reference src/MonitoringPlatform.Domain/MonitoringPlatform.Domain.csproj

dotnet add tests/MonitoringPlatform.IntegrationTests/MonitoringPlatform.IntegrationTests.csproj reference src/MonitoringPlatform.API/MonitoringPlatform.API.csproj
dotnet add tests/MonitoringPlatform.IntegrationTests/MonitoringPlatform.IntegrationTests.csproj reference src/MonitoringPlatform.Infrastructure/MonitoringPlatform.Infrastructure.csproj

# Remove default weather controller and file
rm src/MonitoringPlatform.API/Controllers/WeatherForecastController.cs
rm src/MonitoringPlatform.API/WeatherForecast.cs
```

## NuGet Packages

```bash
# API Project
dotnet add src/MonitoringPlatform.API/MonitoringPlatform.API.csproj package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/MonitoringPlatform.API/MonitoringPlatform.API.csproj package Swashbuckle.AspNetCore
dotnet add src/MonitoringPlatform.API/MonitoringPlatform.API.csproj package Swashbuckle.AspNetCore.Annotations

# Application Project
dotnet add src/MonitoringPlatform.Application/MonitoringPlatform.Application.csproj package MediatR
dotnet add src/MonitoringPlatform.Application/MonitoringPlatform.Application.csproj package FluentValidation
dotnet add src/MonitoringPlatform.Application/MonitoringPlatform.Application.csproj package FluentValidation.DependencyInjectionExtensions

# Infrastructure Project
dotnet add src/MonitoringPlatform.Infrastructure/MonitoringPlatform.Infrastructure.csproj package Microsoft.EntityFrameworkCore
dotnet add src/MonitoringPlatform.Infrastructure/MonitoringPlatform.Infrastructure.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/MonitoringPlatform.Infrastructure/MonitoringPlatform.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Relational
dotnet add src/MonitoringPlatform.Infrastructure/MonitoringPlatform.Infrastructure.csproj package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/MonitoringPlatform.Infrastructure/MonitoringPlatform.Infrastructure.csproj package BCrypt.Net-Next
dotnet add src/MonitoringPlatform.Infrastructure/MonitoringPlatform.Infrastructure.csproj package Microsoft.Extensions.Configuration.Abstractions
dotnet add src/MonitoringPlatform.Infrastructure/MonitoringPlatform.Infrastructure.csproj package Microsoft.Extensions.DependencyInjection.Abstractions
dotnet add src/MonitoringPlatform.Infrastructure/MonitoringPlatform.Infrastructure.csproj package Microsoft.Extensions.Options.ConfigurationExtensions

# Unit Tests
dotnet add tests/MonitoringPlatform.UnitTests/MonitoringPlatform.UnitTests.csproj package Moq
dotnet add tests/MonitoringPlatform.UnitTests/MonitoringPlatform.UnitTests.csproj package FluentAssertions

# Integration Tests
dotnet add tests/MonitoringPlatform.IntegrationTests/MonitoringPlatform.IntegrationTests.csproj package Microsoft.AspNetCore.Mvc.Testing
dotnet add tests/MonitoringPlatform.IntegrationTests/MonitoringPlatform.IntegrationTests.csproj package Moq
dotnet add tests/MonitoringPlatform.IntegrationTests/MonitoringPlatform.IntegrationTests.csproj package FluentAssertions
dotnet add tests/MonitoringPlatform.IntegrationTests/MonitoringPlatform.IntegrationTests.csproj package Microsoft.EntityFrameworkCore
dotnet add tests/MonitoringPlatform.IntegrationTests/MonitoringPlatform.IntegrationTests.csproj package Microsoft.EntityFrameworkCore.InMemory
```

## EF Core Migrations

```bash
# Add migrations
dotnet tool install --global dotnet-ef

dotnet ef migrations add InitialCreate --project src/MonitoringPlatform.Infrastructure --startup-project src/MonitoringPlatform.API

# Update database
dotnet ef database update --project src/MonitoringPlatform.Infrastructure --startup-project src/MonitoringPlatform.API
```

## Run Application

```bash
# Build solution
dotnet build

# Run API
dotnet run --project src/MonitoringPlatform.API
```
