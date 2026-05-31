using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitoringPlatform.Domain.Entities;

namespace MonitoringPlatform.Infrastructure.Data.Configurations;

public class MonitorLogConfiguration : IEntityTypeConfiguration<MonitorLog>
{
    public void Configure(EntityTypeBuilder<MonitorLog> builder)
    {
        builder.HasKey(l => l.LogId);

        builder.Property(l => l.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(l => l.ResponseBody)
            .HasMaxLength(5000);

        builder.HasIndex(l => l.MonitorId);
        builder.HasIndex(l => l.CheckedAt);

        builder.HasOne(l => l.Monitor)
            .WithMany(m => m.MonitorLogs)
            .HasForeignKey(l => l.MonitorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Keep only last 30 days of logs by default (can be configured)
        //builder.ToTable(t => t.HasCheckConstraint("CK_MonitorLog_CheckedAt", "CheckedAt >= DATE_SUB(CURRENT_DATE, INTERVAL 30 DAY)"));
    }
}