using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitoringPlatform.Domain.Entities;

namespace MonitoringPlatform.Infrastructure.Data.Configurations;

public class MonitorCategoryConfiguration : IEntityTypeConfiguration<MonitorCategory>
{
    public void Configure(EntityTypeBuilder<MonitorCategory> builder)
    {
        builder.HasKey(c => c.CategoryId);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.Color)
            .HasMaxLength(7);

        builder.HasIndex(c => new { c.OrganizationId, c.Name })
            .IsUnique();

        builder.HasOne(c => c.Organization)
            .WithMany(o => o.MonitorCategories)
            .HasForeignKey(c => c.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}