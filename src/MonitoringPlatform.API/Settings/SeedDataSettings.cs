using System.ComponentModel.DataAnnotations;

namespace MonitoringPlatform.API.Settings;

public class SeedDataSettings
{
    public const string SectionName = "SeedDataSettings";

    public bool EnableSeeding { get; set; } = true;

    [Required(ErrorMessage = "Default admin email is required")]
    [EmailAddress(ErrorMessage = "Default admin email must be a valid email address")]
    public string DefaultAdminEmail { get; set; } = "admin@example.com";

    [Required(ErrorMessage = "Default admin password is required")]
    [MinLength(6, ErrorMessage = "Default admin password must be at least 6 characters")]
    public string DefaultAdminPassword { get; set; } = "Admin@123";

    public string DefaultOrganizationName { get; set; } = "Default Organization";

    public List<string> SampleMonitorUrls { get; set; } = new()
    {
        "https://example.com",
        "https://api.example.com"
    };
}
