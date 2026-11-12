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
    public DbSet<BillingAccount> BillingAccounts => Set<BillingAccount>();
    public DbSet<OrganizationInvoice> OrganizationInvoices => Set<OrganizationInvoice>();
    public DbSet<InvoiceLineItem> InvoiceLineItems => Set<InvoiceLineItem>();
    public DbSet<InvoicePayment> InvoicePayments => Set<InvoicePayment>();

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
            entity.HasOne(item => item.BillingAccount)
                .WithOne(item => item.Organization)
                .HasForeignKey<BillingAccount>(item => item.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
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
            entity.Property(item => item.PriceAmount).HasPrecision(18, 4);
            entity.Property(item => item.CurrencyCode).HasMaxLength(3);
            entity.Property(item => item.BillingInterval).HasMaxLength(16);
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

        modelBuilder.Entity<BillingAccount>(entity =>
        {
            entity.ToTable("billing_accounts");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.LegalName).HasMaxLength(200).IsRequired();
            entity.Property(item => item.BillingEmail).HasMaxLength(320).IsRequired();
            entity.Property(item => item.ContactName).HasMaxLength(200);
            entity.Property(item => item.Phone).HasMaxLength(40);
            entity.Property(item => item.AddressLine1).HasMaxLength(200);
            entity.Property(item => item.AddressLine2).HasMaxLength(200);
            entity.Property(item => item.City).HasMaxLength(120);
            entity.Property(item => item.Region).HasMaxLength(120);
            entity.Property(item => item.PostalCode).HasMaxLength(32);
            entity.Property(item => item.CountryCode).HasMaxLength(2);
            entity.Property(item => item.TaxIdentifier).HasMaxLength(64);
            entity.HasIndex(item => item.OrganizationId).IsUnique();
            entity.HasMany(item => item.Invoices)
                .WithOne(item => item.BillingAccount)
                .HasForeignKey(item => item.BillingAccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrganizationInvoice>(entity =>
        {
            entity.ToTable("organization_invoices");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.InvoiceNumber).HasMaxLength(32).IsRequired();
            entity.Property(item => item.Status).HasMaxLength(24).IsRequired();
            entity.Property(item => item.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(item => item.BillToName).HasMaxLength(200).IsRequired();
            entity.Property(item => item.BillToEmail).HasMaxLength(320).IsRequired();
            entity.Property(item => item.BillToAddress).HasMaxLength(1200).IsRequired();
            entity.Property(item => item.Subtotal).HasPrecision(18, 4);
            entity.Property(item => item.Total).HasPrecision(18, 4);
            entity.HasIndex(item => new { item.BillingAccountId, item.InvoiceNumber }).IsUnique();
            entity.HasIndex(item => new { item.BillingAccountId, item.CurrencyCode })
                .IsUnique()
                .HasFilter("status IN ('Issued', 'PartiallyPaid')");
            entity.HasMany(item => item.LineItems)
                .WithOne(item => item.Invoice)
                .HasForeignKey(item => item.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(item => item.Payments)
                .WithOne(item => item.Invoice)
                .HasForeignKey(item => item.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InvoiceLineItem>(entity =>
        {
            entity.ToTable("invoice_line_items");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.ProductName).HasMaxLength(160).IsRequired();
            entity.Property(item => item.PlanName).HasMaxLength(160).IsRequired();
            entity.Property(item => item.BillingInterval).HasMaxLength(16).IsRequired();
            entity.Property(item => item.UnitAmount).HasPrecision(18, 4);
            entity.Property(item => item.LineAmount).HasPrecision(18, 4);
            entity.HasIndex(item => item.InvoiceId);
        });

        modelBuilder.Entity<InvoicePayment>(entity =>
        {
            entity.ToTable("invoice_payments");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Amount).HasPrecision(18, 4);
            entity.Property(item => item.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(item => item.ExternalReference).HasMaxLength(200);
            entity.Property(item => item.RecordedByUserId).HasMaxLength(450).IsRequired();
            entity.HasIndex(item => item.InvoiceId);
        });
    }
}
