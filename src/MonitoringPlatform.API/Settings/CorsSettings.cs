using System.ComponentModel.DataAnnotations;

namespace MonitoringPlatform.API.Settings;

public class CorsSettings
{
    public const string SectionName = "CorsSettings";

    [Required(ErrorMessage = "At least one allowed origin is required")]
    [MinLength(1, ErrorMessage = "At least one allowed origin is required")]
    public List<string> AllowedOrigins { get; set; } = new();

    public List<string> AllowedMethods { get; set; } = new() { "GET", "POST", "PUT", "DELETE", "OPTIONS" };

    public List<string> AllowedHeaders { get; set; } = ["*"];

    public bool AllowCredentials { get; set; } = true;

    public int? MaxAgeSeconds { get; set; }
}
