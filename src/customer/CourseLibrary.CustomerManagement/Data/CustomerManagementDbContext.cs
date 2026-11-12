using CourseLibrary.CustomerManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseLibrary.CustomerManagement.Data;

public sealed class CustomerManagementDbContext(DbContextOptions<CustomerManagementDbContext> options)
    : DbContext(options)
{
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationMembership> OrganizationMemberships => Set<OrganizationMembership>();
    public DbSet<OrganizationProductAccess> OrganizationProductAccess => Set<OrganizationProductAccess>();
    public DbSet<OrganizationInvitation> OrganizationInvitations => Set<OrganizationInvitation>();
    public DbSet<OrganizationInvitationProduct> OrganizationInvitationProducts => Set<OrganizationInvitationProduct>();
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

        modelBuilder.Entity<OrganizationProductAccess>(entity =>
        {
            entity.ToTable("organization_product_access");
            entity.HasKey(item => new { item.OrganizationId, item.UserId, item.ProductId });
            entity.Property(item => item.UserId).HasMaxLength(450).IsRequired();
            entity.HasOne(item => item.Membership)
                .WithMany(item => item.ProductAccess)
                .HasForeignKey(item => new { item.OrganizationId, item.UserId })
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.Product)
                .WithMany()
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
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

        modelBuilder.Entity<OrganizationInvitationProduct>(entity =>
        {
            entity.ToTable("organization_invitation_products");
            entity.HasKey(item => new { item.InvitationId, item.ProductId });
            entity.HasOne(item => item.Invitation)
                .WithMany(item => item.Products)
                .HasForeignKey(item => item.InvitationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.Product)
                .WithMany()
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
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
            entity.HasAlternateKey(item => new { item.ProductId, item.Id });
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
            entity.HasIndex(item => new { item.OrganizationId, item.ProductId }).IsUnique();
            entity.HasOne(item => item.ProductPlan)
                .WithMany(item => item.Subscriptions)
                .HasForeignKey(item => new { item.ProductId, item.ProductPlanId })
                .HasPrincipalKey(item => new { item.ProductId, item.Id })
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.Organization)
                .WithMany(item => item.Subscriptions)
                .HasForeignKey(item => item.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
