using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Enums;

namespace MonitoringPlatform.Infrastructure.Data.Configurations;

public class AlertRuleConfiguration : IEntityTypeConfiguration<AlertRule>
{
    public void Configure(EntityTypeBuilder<AlertRule> builder)
    {
        builder.HasKey(ar => ar.RuleId);

        builder.Property(ar => ar.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ar => ar.ConditionType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(ar => ar.ThresholdValue)
            .HasMaxLength(50);

        builder.Property(ar => ar.Severity)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(ar => ar.CooldownMinutes)
            .IsRequired()
            .HasDefaultValue(5);

        builder.HasIndex(ar => new { ar.MonitorId, ar.ConditionType })
            .IsUnique();

        builder.HasOne(ar => ar.Organization)
            .WithMany()
            .HasForeignKey(ar => ar.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ar => ar.Monitor)
            .WithMany()
            .HasForeignKey(ar => ar.MonitorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(ar => ar.NotificationChannels)
            .WithMany(nc => nc.AlertRules)
            .UsingEntity<Dictionary<string, object>>(
                "AlertRuleNotificationChannel",
                j => j.HasOne<NotificationChannel>()
                    .WithMany()
                    .HasForeignKey("ChannelId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<AlertRule>()
                    .WithMany()
                    .HasForeignKey("RuleId")
                    .OnDelete(DeleteBehavior.Cascade));
    }
}
