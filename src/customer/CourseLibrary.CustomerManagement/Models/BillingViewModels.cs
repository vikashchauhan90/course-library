using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.CustomerManagement.Models;

public sealed class BillingAccountViewModel
{
    public Guid OrganizationId { get; set; }
    public bool IsSaved { get; set; }

    [Required, StringLength(200)]
    public string LegalName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(320)]
    public string BillingEmail { get; set; } = string.Empty;

    [StringLength(200)]
    public string? ContactName { get; set; }

    [StringLength(40)]
    public string? Phone { get; set; }

    [StringLength(200)]
    public string? AddressLine1 { get; set; }

    [StringLength(200)]
    public string? AddressLine2 { get; set; }

    [StringLength(120)]
    public string? City { get; set; }

    [StringLength(120)]
    public string? Region { get; set; }

    [StringLength(32)]
    public string? PostalCode { get; set; }

    [StringLength(2)]
    public string? CountryCode { get; set; }

    [StringLength(64)]
    public string? TaxIdentifier { get; set; }
}

public sealed record InvoiceLineItemViewModel(
    string ProductName,
    string PlanName,
    string BillingInterval,
    int Quantity,
    decimal UnitAmount,
    decimal LineAmount,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd);

public sealed record InvoicePaymentViewModel(
    decimal Amount,
    string CurrencyCode,
    string? ExternalReference,
    DateTimeOffset RecordedAt);

public sealed record InvoiceListItemViewModel(
    Guid Id,
    string InvoiceNumber,
    string CurrencyCode,
    string Status,
    decimal Subtotal,
    decimal Total,
    decimal AmountPaid,
    DateTimeOffset IssuedAt,
    DateTimeOffset DueAt,
    IReadOnlyList<InvoiceLineItemViewModel> LineItems,
    IReadOnlyList<InvoicePaymentViewModel> Payments);

public sealed class BillingInvoicesViewModel
{
    public Guid OrganizationId { get; init; }
    public string OrganizationName { get; init; } = string.Empty;
    public bool HasBillingAccount { get; init; }
    public IReadOnlyList<InvoiceListItemViewModel> Invoices { get; init; } = [];
}

public sealed class RecordInvoicePaymentViewModel
{
    [Range(typeof(decimal), "0.0001", "999999999999")]
    public decimal Amount { get; set; }

    [StringLength(200)]
    public string? ExternalReference { get; set; }
}
