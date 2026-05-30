using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Enums;
using MonitoringPlatform.Infrastructure.Data;
using BCrypt.Net;

namespace MonitoringPlatform.API.Data;

public static class DbSeed
{
    public static async Task SeedAsync(ApplicationDbContext context)
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
            Name = "Default Organization",
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
            Email = "admin@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
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
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Console.WriteLine(ex.InnerException?.Message);
            throw;
        }
    }
}