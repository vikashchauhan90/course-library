namespace CourseLibrary.CustomerManagement.Domain.Entities;

public sealed class InvoiceLineItem
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid ProductSubscriptionId { get; set; }
    public required string ProductName { get; set; }
    public required string PlanName { get; set; }
    public required string BillingInterval { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitAmount { get; set; }
    public decimal LineAmount { get; set; }
    public DateTimeOffset PeriodStart { get; set; }
    public DateTimeOffset PeriodEnd { get; set; }
    public OrganizationInvoice? Invoice { get; set; }
}
