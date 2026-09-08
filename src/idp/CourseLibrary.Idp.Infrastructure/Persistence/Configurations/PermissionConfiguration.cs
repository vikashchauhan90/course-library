using CourseLibrary.Idp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourseLibrary.Idp.Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions", "identity");
        builder.HasKey(permission => permission.Id);
        builder.Property(permission => permission.Id).HasMaxLength(100);
        builder.Property(permission => permission.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(permission => permission.Resource).HasMaxLength(100).IsRequired();
        builder.Property(permission => permission.Action).HasMaxLength(50).IsRequired();
        builder.HasIndex(permission => new { permission.Resource, permission.Action }).IsUnique();
    }
}