using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitoringPlatform.Domain.Entities;

namespace MonitoringPlatform.Infrastructure.Data.Configurations;

public class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.HasKey(a => a.AlertId);

        builder.Property(a => a.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.HasIndex(a => a.MonitorId);

        builder.HasOne(a => a.Monitor)
            .WithMany(m => m.Alerts)
            .HasForeignKey(a => a.MonitorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}