using CourseLibrary.Idp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourseLibrary.Idp.Infrastructure.Persistence.Configurations;

public sealed class OpenIddictApplicationConfiguration
    : IEntityTypeConfiguration<OpenIddictApplication>
{
    public void Configure(EntityTypeBuilder<OpenIddictApplication> builder)
    {
        builder.ToTable("applications", "oauth");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.DeletedAt);

        builder.Property(x => x.ConcurrencyStamp)
            .HasMaxLength(64)
            .IsConcurrencyToken();

        builder.HasIndex(x => x.ClientId)
            .IsUnique();

        builder.HasIndex(x => x.ConcurrencyStamp);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.DeletedAt);
    }
}