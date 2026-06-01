using System.ComponentModel.DataAnnotations;

namespace MonitoringPlatform.Infrastructure.Settings;

public class JwtAuthenticationSettings
{
    public const string SectionName = "JwtAuthenticationSettings";

    [Range(0, 300, ErrorMessage = "ClockSkewSeconds must be between 0 and 300")]
    public int ClockSkewSeconds { get; set; } = 0;

    public bool ValidateIssuer { get; set; } = true;

    public bool ValidateAudience { get; set; } = true;

    public bool ValidateLifetime { get; set; } = true;

    public bool ValidateIssuerSigningKey { get; set; } = true;
}
