using CourseLibrary.Idp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourseLibrary.Idp.Infrastructure.Persistence.Configurations;

public sealed class OpenIddictAuthorizationConfiguration
    : IEntityTypeConfiguration<OpenIddictAuthorization>
{
    public void Configure(EntityTypeBuilder<OpenIddictAuthorization> builder)
    {
        builder.ToTable("authorizations", "oauth");

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

        builder.HasIndex(x => x.ConcurrencyStamp);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.DeletedAt);
    }
}