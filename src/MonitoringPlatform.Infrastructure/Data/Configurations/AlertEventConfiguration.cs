using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Enums;

namespace MonitoringPlatform.Infrastructure.Data.Configurations;

public class AlertEventConfiguration : IEntityTypeConfiguration<AlertEvent>
{
    public void Configure(EntityTypeBuilder<AlertEvent> builder)
    {
        builder.HasKey(ae => ae.EventId);

        builder.Property(ae => ae.Severity)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(ae => ae.ConditionType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(ae => ae.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(ae => ae.AttemptCount)
            .HasDefaultValue(0);

        builder.Property(ae => ae.IsNotificationSent)
            .HasDefaultValue(false);

        builder.HasIndex(ae => new { ae.OrganizationId, ae.MonitorId, ae.TriggeredAt });
        builder.HasIndex(ae => ae.IsNotificationSent);

        builder.HasOne(ae => ae.Organization)
            .WithMany()
            .HasForeignKey(ae => ae.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ae => ae.Monitor)
            .WithMany()
            .HasForeignKey(ae => ae.MonitorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ae => ae.AlertRule)
            .WithMany()
            .HasForeignKey(ae => ae.AlertRuleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
