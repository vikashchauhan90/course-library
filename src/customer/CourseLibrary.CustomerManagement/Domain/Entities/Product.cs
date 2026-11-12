namespace CourseLibrary.CustomerManagement.Domain.Entities;

public sealed class Product
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; }
    public ICollection<ProductPlan> Plans { get; set; } = [];
}
