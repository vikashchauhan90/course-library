namespace CourseLibrary.CustomerManagement.Domain.Entities;

public sealed class ProductPlan
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; }
    public Product? Product { get; set; }
    public ICollection<OrganizationSubscription> Subscriptions { get; set; } = [];
}
