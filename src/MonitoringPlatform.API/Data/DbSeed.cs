using MonitoringPlatform.API.Settings;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Enums;
using MonitoringPlatform.Infrastructure.Data;
using BCrypt.Net;

namespace MonitoringPlatform.API.Data;

public static class DbSeed
{
    public static async Task SeedAsync(ApplicationDbContext context, SeedDataSettings settings)
    {
        // Check if data already exists
        if (context.Organizations.Any())
        {
            return;
        }

        // Create default organization
        var defaultOrg = new Organization
        {
            OrganizationId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = settings.DefaultOrganizationName,
            Slug = "default-org",
            Status = OrganizationStatus.Active,
            PlanType = PlanType.Starter,
            MaxMonitors = 10,
            MaxAlerts = 3,
            CreatedAt = DateTime.UtcNow
        };

        context.Organizations.Add(defaultOrg);
        await context.SaveChangesAsync();

        // Create admin user
        var adminUser = new User
        {
            UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            OrganizationId = defaultOrg.OrganizationId,
            Email = settings.DefaultAdminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(settings.DefaultAdminPassword),
            FirstName = "Admin",
            LastName = "User",
            Role = UserRole.Owner,
            Status = UserStatus.Active,
            EmailVerified = true,
            EmailVerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            context.Users.Add(adminUser);
            await context.SaveChangesAsync();

            // Seed Categories
            var prodCategory = new MonitorCategory
            {
                CategoryId = Guid.NewGuid(),
                OrganizationId = defaultOrg.OrganizationId,
                Name = "Production",
                Description = "Production environment monitors",
                Color = "#EF4444",
                CreatedAt = DateTime.UtcNow
            };

            var devCategory = new MonitorCategory
            {
                CategoryId = Guid.NewGuid(),
                OrganizationId = defaultOrg.OrganizationId,
                Name = "Development",
                Description = "Development environment monitors",
                Color = "#3B82F6",
                CreatedAt = DateTime.UtcNow
            };

            context.MonitorCategories.AddRange(prodCategory, devCategory);
            await context.SaveChangesAsync();

            // Seed Tags
            var criticalTag = new MonitorTag
            {
                TagId = Guid.NewGuid(),
                OrganizationId = defaultOrg.OrganizationId,
                Name = "Critical",
                Color = "#DC2626",
                CreatedAt = DateTime.UtcNow
            };

            var apiTag = new MonitorTag
            {
                TagId = Guid.NewGuid(),
                OrganizationId = defaultOrg.OrganizationId,
                Name = "API",
                Color = "#10B981",
                CreatedAt = DateTime.UtcNow
            };

            context.MonitorTags.AddRange(criticalTag, apiTag);
            await context.SaveChangesAsync();

            // Seed Monitors using configured URLs
            if (settings.SampleMonitorUrls.Any())
            {
                var monitor1 = new Domain.Entities.Monitor
                {
                    MonitorId = Guid.NewGuid(),
                    OrganizationId = defaultOrg.OrganizationId,
                    Name = "Google Ping",
                    Description = "Pinging Google DNS",
                    Type = MonitorType.Ping,
                    Target = "8.8.8.8",
                    IntervalSeconds = 60,
                    TimeoutSeconds = 10,
                    Status = MonitorStatus.Active,
                    CreatedAt = DateTime.UtcNow
                };

                var monitor2 = new Domain.Entities.Monitor
                {
                    MonitorId = Guid.NewGuid(),
                    OrganizationId = defaultOrg.OrganizationId,
                    Name = "Sample Website",
                    Description = "Checking sample website availability",
                    Type = MonitorType.Https,
                    Target = settings.SampleMonitorUrls.First(),
                    IntervalSeconds = 120,
                    TimeoutSeconds = 30,
                    Status = MonitorStatus.Active,
                    CreatedAt = DateTime.UtcNow
                };

                monitor1.MonitorCategories.Add(prodCategory);
                monitor1.MonitorTags.Add(criticalTag);

                monitor2.MonitorCategories.Add(devCategory);
                monitor2.MonitorTags.Add(apiTag);

                context.Monitors.AddRange(monitor1, monitor2);
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Console.WriteLine(ex.InnerException?.Message);
            throw;
        }
    }
}
