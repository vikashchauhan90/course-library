namespace CourseLibrary.CustomerManagement.Domain.Entities;

public sealed class OrganizationInvoice
{
    public Guid Id { get; set; }
    public Guid BillingAccountId { get; set; }
    public required string InvoiceNumber { get; set; }
    public required string Status { get; set; }
    public required string CurrencyCode { get; set; }
    public required string BillToName { get; set; }
    public required string BillToEmail { get; set; }
    public string BillToAddress { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public DateTimeOffset IssuedAt { get; set; }
    public DateTimeOffset DueAt { get; set; }
    public BillingAccount? BillingAccount { get; set; }
    public ICollection<InvoiceLineItem> LineItems { get; set; } = [];
    public ICollection<InvoicePayment> Payments { get; set; } = [];
}
