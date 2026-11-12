namespace CourseLibrary.CustomerManagement.Domain.Entities;

public sealed class OrganizationSubscription
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid ProductId { get; set; }
    public Guid ProductPlanId { get; set; }
    public required string Status { get; set; }
    public DateTimeOffset SelectedAt { get; set; }
    public DateTimeOffset NextInvoiceAt { get; set; }
    public Organization? Organization { get; set; }
    public ProductPlan? ProductPlan { get; set; }
}
