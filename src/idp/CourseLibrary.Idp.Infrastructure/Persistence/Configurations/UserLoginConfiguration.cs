using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourseLibrary.Idp.Infrastructure.Persistence.Configurations;

public sealed class UserLoginConfiguration : IEntityTypeConfiguration<IdentityUserLogin<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserLogin<string>> builder)
    {
        builder.ToTable("user_logins", "identity");
        builder.HasKey(l => new { l.LoginProvider, l.ProviderKey });
    }
}