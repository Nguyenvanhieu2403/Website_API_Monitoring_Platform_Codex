using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitoringPlatform.Domain.Entities;

namespace MonitoringPlatform.Infrastructure.Data.Configurations;

public class MonitorTagConfiguration : IEntityTypeConfiguration<MonitorTag>
{
    public void Configure(EntityTypeBuilder<MonitorTag> builder)
    {
        builder.HasKey(t => t.TagId);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Color)
            .HasMaxLength(7);

        builder.HasIndex(t => new { t.OrganizationId, t.Name })
            .IsUnique();

        builder.HasOne(t => t.Organization)
            .WithMany(o => o.MonitorTags)
            .HasForeignKey(t => t.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}