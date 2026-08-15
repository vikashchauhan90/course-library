using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourseLibrary.Idp.Infrastructure.Persistence.Configurations;

public sealed class RoleClaimConfiguration : IEntityTypeConfiguration<IdentityRoleClaim<string>>
{
    public void Configure(EntityTypeBuilder<IdentityRoleClaim<string>> builder)
    {
        builder.ToTable("role_claims");
        builder.HasKey(rc => rc.Id);
    }
}