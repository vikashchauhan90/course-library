namespace CourseLibrary.CustomerManagement.Domain.Entities;

public sealed class InvoicePayment
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public required string CurrencyCode { get; set; }
    public string? ExternalReference { get; set; }
    public required string RecordedByUserId { get; set; }
    public DateTimeOffset RecordedAt { get; set; }
    public OrganizationInvoice? Invoice { get; set; }
}
