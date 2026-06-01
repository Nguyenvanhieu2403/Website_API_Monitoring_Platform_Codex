using System.ComponentModel.DataAnnotations;

namespace MonitoringPlatform.Infrastructure.Settings;

public class NotificationWorkerSettings
{
    public const string SectionName = "NotificationWorkerSettings";

    [Range(1, 10, ErrorMessage = "MaxRetryAttempts must be between 1 and 10")]
    public int MaxRetryAttempts { get; set; } = 3;

    [Range(5, 300, ErrorMessage = "PollingIntervalSeconds must be between 5 and 300")]
    public int PollingIntervalSeconds { get; set; } = 30;

    public bool Enabled { get; set; } = true;
}
