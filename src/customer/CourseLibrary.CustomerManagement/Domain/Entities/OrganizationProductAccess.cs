namespace CourseLibrary.CustomerManagement.Domain.Entities;

public sealed class OrganizationProductAccess
{
    public Guid OrganizationId { get; set; }
    public required string UserId { get; set; }
    public Guid ProductId { get; set; }
    public DateTimeOffset GrantedAt { get; set; }
    public OrganizationMembership? Membership { get; set; }
    public Product? Product { get; set; }
}
