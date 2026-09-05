using CourseLibrary.Idp.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CourseLibrary.Idp.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<UserInvitation> UserInvitations => Set<UserInvitation>();
    public DbSet<OpenIddictToken> OpenIddictTokens => Set<OpenIddictToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure OpenIddict entities
        builder.UseOpenIddict<
                OpenIddictApplication,
                OpenIddictAuthorization,
                OpenIddictScope,
                OpenIddictToken,
                Guid>();

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Seeding via dedicated seeder class
        InitialDataSeeder.SeedData(builder);
    }

    public Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        return Database.MigrateAsync(cancellationToken);
    }
}
