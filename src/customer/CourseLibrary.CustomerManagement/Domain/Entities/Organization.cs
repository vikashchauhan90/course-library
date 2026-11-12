namespace CourseLibrary.CustomerManagement.Domain.Entities;

public sealed class Organization
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public ICollection<OrganizationMembership> Memberships { get; set; } = [];
    public ICollection<OrganizationSubscription> Subscriptions { get; set; } = [];
    public BillingAccount? BillingAccount { get; set; }
}
