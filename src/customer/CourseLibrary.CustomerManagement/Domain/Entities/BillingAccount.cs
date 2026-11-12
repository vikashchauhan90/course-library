namespace CourseLibrary.CustomerManagement.Domain.Entities;

public sealed class BillingAccount
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public required string LegalName { get; set; }
    public required string BillingEmail { get; set; }
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string? CountryCode { get; set; }
    public string? TaxIdentifier { get; set; }
    public int NextInvoiceNumber { get; set; } = 1;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Organization? Organization { get; set; }
    public ICollection<OrganizationInvoice> Invoices { get; set; } = [];
}
