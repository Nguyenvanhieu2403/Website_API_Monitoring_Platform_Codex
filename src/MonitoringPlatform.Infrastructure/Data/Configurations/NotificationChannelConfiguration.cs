using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Enums;

namespace MonitoringPlatform.Infrastructure.Data.Configurations;

public class NotificationChannelConfiguration : IEntityTypeConfiguration<NotificationChannel>
{
    public void Configure(EntityTypeBuilder<NotificationChannel> builder)
    {
        builder.HasKey(nc => nc.ChannelId);

        builder.Property(nc => nc.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(nc => nc.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(nc => nc.Configuration)
            .IsRequired()
            .HasColumnType("jsonb"); // Use jsonb for PostgreSQL to store JSON configurations

        builder.HasIndex(nc => new { nc.OrganizationId, nc.Name })
            .IsUnique();

        builder.HasOne(nc => nc.Organization)
            .WithMany()
            .HasForeignKey(nc => nc.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-many relationship with AlertRule is configured in AlertRuleConfiguration
    }
}
