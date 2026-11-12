using CourseLibrary.CustomerManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseLibrary.CustomerManagement.Data;

public sealed class CustomerManagementDbContext(DbContextOptions<CustomerManagementDbContext> options)
    : DbContext(options)
{
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationMembership> OrganizationMemberships => Set<OrganizationMembership>();
    public DbSet<OrganizationInvitation> OrganizationInvitations => Set<OrganizationInvitation>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductPlan> ProductPlans => Set<ProductPlan>();
    public DbSet<OrganizationSubscription> OrganizationSubscriptions => Set<OrganizationSubscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("customer_management");
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.ToTable("organizations");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Name).HasMaxLength(200).IsRequired();
            entity.Property(item => item.CreatedByUserId).HasMaxLength(450).IsRequired();
            entity.HasIndex(item => new { item.CreatedByUserId, item.Name });
        });

        modelBuilder.Entity<OrganizationMembership>(entity =>
        {
            entity.ToTable("organization_memberships");
            entity.HasKey(item => new { item.OrganizationId, item.UserId });
            entity.Property(item => item.UserId).HasMaxLength(450).IsRequired();
            entity.Property(item => item.Email).HasMaxLength(320).IsRequired();
            entity.Property(item => item.NormalizedEmail).HasMaxLength(320).IsRequired();
            entity.Property(item => item.Role).HasMaxLength(32).IsRequired();
            entity.HasIndex(item => new { item.OrganizationId, item.NormalizedEmail }).IsUnique();
            entity.HasOne(item => item.Organization)
                .WithMany(item => item.Memberships)
                .HasForeignKey(item => item.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrganizationInvitation>(entity =>
        {
            entity.ToTable("organization_invitations");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Email).HasMaxLength(320).IsRequired();
            entity.Property(item => item.NormalizedEmail).HasMaxLength(320).IsRequired();
            entity.Property(item => item.Role).HasMaxLength(32).IsRequired();
            entity.Property(item => item.InvitedByUserId).HasMaxLength(450).IsRequired();
            entity.Property(item => item.TokenHash).HasMaxLength(64).IsRequired();
            entity.HasIndex(item => item.TokenHash).IsUnique();
            entity.HasIndex(item => new { item.OrganizationId, item.NormalizedEmail });
            entity.HasOne(item => item.Organization)
                .WithMany()
                .HasForeignKey(item => item.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Code).HasMaxLength(80).IsRequired();
            entity.Property(item => item.Name).HasMaxLength(160).IsRequired();
            entity.HasIndex(item => item.Code).IsUnique();
        });

        modelBuilder.Entity<ProductPlan>(entity =>
        {
            entity.ToTable("product_plans");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Code).HasMaxLength(80).IsRequired();
            entity.Property(item => item.Name).HasMaxLength(160).IsRequired();
            entity.HasIndex(item => new { item.ProductId, item.Code }).IsUnique();
            entity.HasOne(item => item.Product)
                .WithMany(item => item.Plans)
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrganizationSubscription>(entity =>
        {
            entity.ToTable("organization_subscriptions");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(item => new { item.OrganizationId, item.ProductPlanId }).IsUnique();
            entity.HasOne(item => item.Organization)
                .WithMany(item => item.Subscriptions)
                .HasForeignKey(item => item.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.ProductPlan)
                .WithMany(item => item.Subscriptions)
                .HasForeignKey(item => item.ProductPlanId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
