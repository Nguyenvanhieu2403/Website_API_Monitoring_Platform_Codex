using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitoringPlatform.Domain.Entities;

namespace MonitoringPlatform.Infrastructure.Data.Configurations;

public class MonitorConfiguration : IEntityTypeConfiguration<Domain.Entities.Monitor>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Monitor> builder)
    {
        builder.HasKey(m => m.MonitorId);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Description)
            .HasMaxLength(500);

        builder.Property(m => m.Target)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(m => m.HttpMethod)
            .HasMaxLength(10);

        builder.Property(m => m.ExpectedStatusCode)
            .HasMaxLength(10);

        builder.Property(m => m.ExpectedKeyword)
            .HasMaxLength(500);

        builder.HasIndex(m => new { m.OrganizationId, m.IsDeleted });

        builder.HasQueryFilter(m => !m.IsDeleted);

        builder.HasOne(m => m.Organization)
            .WithMany(o => o.Monitors)
            .HasForeignKey(m => m.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(m => m.MonitorCategories)
            .WithMany(mc => mc.Monitors)
            .UsingEntity<Dictionary<string, object>>(
                "MonitorCategoryMonitor",
                j => j.HasOne<MonitorCategory>()
                    .WithMany()
                    .HasForeignKey("CategoryId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<Domain.Entities.Monitor>()
                    .WithMany()
                    .HasForeignKey("MonitorId")
                    .OnDelete(DeleteBehavior.Cascade));

        builder.HasMany(m => m.MonitorTags)
            .WithMany(mt => mt.Monitors)
            .UsingEntity<Dictionary<string, object>>(
                "MonitorTagMonitor",
                j => j.HasOne<MonitorTag>()
                    .WithMany()
                    .HasForeignKey("TagId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<Domain.Entities.Monitor>()
                    .WithMany()
                    .HasForeignKey("MonitorId")
                    .OnDelete(DeleteBehavior.Cascade));
    }
}
