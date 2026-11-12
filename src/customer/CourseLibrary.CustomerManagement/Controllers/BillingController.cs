using System.Globalization;
using System.Security.Claims;
using System.Text.RegularExpressions;
using CourseLibrary.CustomerManagement.Data;
using CourseLibrary.CustomerManagement.Domain.Entities;
using CourseLibrary.CustomerManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseLibrary.CustomerManagement.Controllers;

[Authorize]
public sealed class BillingController(CustomerManagementDbContext dbContext) : Controller
{
    [HttpGet("organizations/{organizationId:guid}/billing")]
    public async Task<IActionResult> Index(Guid organizationId, CancellationToken cancellationToken)
    {
        var authorization = await CheckManagerAccessAsync(organizationId, cancellationToken);
        if (authorization is not null)
            return authorization;

        var organization = await dbContext.Organizations
            .AsNoTracking()
            .Include(item => item.BillingAccount)
            .SingleOrDefaultAsync(item => item.Id == organizationId, cancellationToken);
        if (organization is null)
            return NotFound();

        var account = organization.BillingAccount;
        return View(new BillingAccountViewModel
        {
            OrganizationId = organizationId,
            IsSaved = account is not null,
            LegalName = account?.LegalName ?? organization.Name,
            BillingEmail = account?.BillingEmail ?? GetUserEmail(),
            ContactName = account?.ContactName,
            Phone = account?.Phone,
            AddressLine1 = account?.AddressLine1,
            AddressLine2 = account?.AddressLine2,
            City = account?.City,
            Region = account?.Region,
            PostalCode = account?.PostalCode,
            CountryCode = account?.CountryCode,
            TaxIdentifier = account?.TaxIdentifier
        });
    }

    [HttpPost("organizations/{organizationId:guid}/billing")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(
        Guid organizationId,
        BillingAccountViewModel model,
        CancellationToken cancellationToken)
    {
        var authorization = await CheckManagerAccessAsync(organizationId, cancellationToken);
        if (authorization is not null)
            return authorization;

        model.OrganizationId = organizationId;
        model.IsSaved = await dbContext.BillingAccounts
            .AnyAsync(item => item.OrganizationId == organizationId, cancellationToken);
        if (!string.IsNullOrWhiteSpace(model.CountryCode) &&
            !Regex.IsMatch(model.CountryCode, "^[A-Za-z]{2}$", RegexOptions.CultureInvariant))
        {
            ModelState.AddModelError(nameof(model.CountryCode), "Enter a two-letter ISO country code.");
        }
        if (!ModelState.IsValid)
            return View("Index", model);

        var account = await dbContext.BillingAccounts
            .SingleOrDefaultAsync(item => item.OrganizationId == organizationId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        if (account is null)
        {
            account = new BillingAccount
            {
                Id = organizationId,
                OrganizationId = organizationId,
                LegalName = model.LegalName.Trim(),
                BillingEmail = model.BillingEmail.Trim(),
                NextInvoiceNumber = 1,
                CreatedAt = now,
                UpdatedAt = now
            };
            dbContext.BillingAccounts.Add(account);
            model.IsSaved = true;
        }

        account.LegalName = model.LegalName.Trim();
        account.BillingEmail = model.BillingEmail.Trim();
        account.ContactName = CleanOptional(model.ContactName);
        account.Phone = CleanOptional(model.Phone);
        account.AddressLine1 = CleanOptional(model.AddressLine1);
        account.AddressLine2 = CleanOptional(model.AddressLine2);
        account.City = CleanOptional(model.City);
        account.Region = CleanOptional(model.Region);
        account.PostalCode = CleanOptional(model.PostalCode);
        account.CountryCode = CleanOptional(model.CountryCode)?.ToUpperInvariant();
        account.TaxIdentifier = CleanOptional(model.TaxIdentifier);
        account.UpdatedAt = now;
        await dbContext.SaveChangesAsync(cancellationToken);

        TempData["BillingSuccess"] = "Billing account saved.";
        return RedirectToAction(nameof(Index), new { organizationId });
    }

    [HttpGet("organizations/{organizationId:guid}/billing/invoices")]
    public async Task<IActionResult> Invoices(Guid organizationId, CancellationToken cancellationToken)
    {
        var authorization = await CheckManagerAccessAsync(organizationId, cancellationToken);
        if (authorization is not null)
            return authorization;

        var organization = await dbContext.Organizations
            .AsNoTracking()
            .Include(item => item.BillingAccount)
            .SingleOrDefaultAsync(item => item.Id == organizationId, cancellationToken);
        if (organization is null)
            return NotFound();

        var invoices = organization.BillingAccount is null
            ? []
            : await dbContext.OrganizationInvoices
                .AsNoTracking()
                .AsSplitQuery()
                .Include(item => item.LineItems)
                .Include(item => item.Payments)
                .Where(item => item.BillingAccountId == organization.BillingAccount.Id)
                .OrderByDescending(item => item.IssuedAt)
                .Select(item => new InvoiceListItemViewModel(
                    item.Id,
                    item.InvoiceNumber,
                    item.CurrencyCode,
                    item.Status,
                    item.Subtotal,
                    item.Total,
                    item.Payments.Sum(payment => (decimal?)payment.Amount) ?? 0,
                    item.IssuedAt,
                    item.DueAt,
                    item.LineItems
                        .OrderBy(line => line.ProductName)
                        .Select(line => new InvoiceLineItemViewModel(
                            line.ProductName,
                            line.PlanName,
                            line.BillingInterval,
                            line.Quantity,
                            line.UnitAmount,
                            line.LineAmount,
                            line.PeriodStart,
                            line.PeriodEnd))
                        .ToList(),
                    item.Payments
                        .OrderBy(payment => payment.RecordedAt)
                        .Select(payment => new InvoicePaymentViewModel(
                            payment.Amount,
                            payment.CurrencyCode,
                            payment.ExternalReference,
                            payment.RecordedAt))
                        .ToList()))
                .ToListAsync(cancellationToken);

        return View(new BillingInvoicesViewModel
        {
            OrganizationId = organizationId,
            OrganizationName = organization.Name,
            HasBillingAccount = organization.BillingAccount is not null,
            Invoices = invoices
        });
    }

    [HttpPost("organizations/{organizationId:guid}/billing/invoices/generate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateInvoice(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var authorization = await CheckManagerAccessAsync(organizationId, cancellationToken);
        if (authorization is not null)
            return authorization;

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var account = await dbContext.BillingAccounts
            .FromSqlInterpolated(
                $"SELECT * FROM customer_management.billing_accounts WHERE organization_id = {organizationId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);
        if (account is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            TempData["BillingError"] = "Save the billing account before generating an invoice.";
            return RedirectToAction(nameof(Invoices), new { organizationId });
        }

        var now = DateTimeOffset.UtcNow;
        var subscriptions = await dbContext.OrganizationSubscriptions
            .Where(item => item.OrganizationId == organizationId &&
                item.Status == "Selected" &&
                item.NextInvoiceAt <= now &&
                item.ProductPlan!.IsActive &&
                item.ProductPlan.Product!.IsActive)
            .Include(item => item.ProductPlan)
                .ThenInclude(item => item!.Product)
            .ToListAsync(cancellationToken);
        if (subscriptions.Count == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            TempData["BillingError"] = "No subscribed products are due for invoicing.";
            return RedirectToAction(nameof(Invoices), new { organizationId });
        }

        var unpricedSubscription = subscriptions.FirstOrDefault(item =>
            item.ProductPlan!.PriceAmount is null ||
            item.ProductPlan.PriceAmount < 0 ||
            (item.ProductPlan.PriceAmount > 0 &&
                (string.IsNullOrWhiteSpace(item.ProductPlan.CurrencyCode) ||
                    !Regex.IsMatch(item.ProductPlan.CurrencyCode, "^[A-Z]{3}$", RegexOptions.CultureInvariant) ||
                    item.ProductPlan.BillingInterval is not ("Monthly" or "Yearly"))));
        if (unpricedSubscription is not null)
        {
            await transaction.RollbackAsync(cancellationToken);
            TempData["BillingError"] =
                $"Configure a valid price, currency, and billing interval for {unpricedSubscription.ProductPlan!.Product!.Name} before invoicing.";
            return RedirectToAction(nameof(Invoices), new { organizationId });
        }

        var billableSubscriptions = subscriptions
            .Where(item => item.ProductPlan!.PriceAmount > 0)
            .GroupBy(item => item.ProductPlan!.CurrencyCode!)
            .ToArray();
        if (billableSubscriptions.Length == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            TempData["BillingError"] = "All subscribed products are free; there is nothing to invoice.";
            return RedirectToAction(nameof(Invoices), new { organizationId });
        }

        foreach (var group in billableSubscriptions)
        {
            var hasOpenInvoice = await dbContext.OrganizationInvoices.AnyAsync(
                item => item.BillingAccountId == account.Id &&
                    item.CurrencyCode == group.Key &&
                    (item.Status == "Issued" || item.Status == "PartiallyPaid"),
                cancellationToken);
            if (hasOpenInvoice)
            {
                await transaction.RollbackAsync(cancellationToken);
                TempData["BillingError"] =
                    $"Settle the open {group.Key} invoice before generating another invoice in that currency.";
                return RedirectToAction(nameof(Invoices), new { organizationId });
            }
        }

        foreach (var group in billableSubscriptions)
        {
            var invoice = new OrganizationInvoice
            {
                Id = Guid.NewGuid(),
                BillingAccountId = account.Id,
                InvoiceNumber = $"INV-{now:yyyy}-{account.NextInvoiceNumber++:D6}",
                Status = "Issued",
                CurrencyCode = group.Key,
                BillToName = account.LegalName,
                BillToEmail = account.BillingEmail,
                BillToAddress = BuildBillingAddress(account),
                IssuedAt = now,
                DueAt = now.AddDays(30)
            };

            foreach (var subscription in group)
            {
                var plan = subscription.ProductPlan!;
                var amount = plan.PriceAmount!.Value;
                var periodStart = subscription.NextInvoiceAt;
                var periodEnd = AddBillingInterval(periodStart, plan.BillingInterval!);
                invoice.LineItems.Add(new InvoiceLineItem
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoice.Id,
                    ProductSubscriptionId = subscription.Id,
                    ProductName = plan.Product!.Name,
                    PlanName = plan.Name,
                    BillingInterval = plan.BillingInterval!,
                    Quantity = 1,
                    UnitAmount = amount,
                    LineAmount = amount,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd
                });
                subscription.NextInvoiceAt = periodEnd;
            }

            invoice.Subtotal = invoice.LineItems.Sum(item => item.LineAmount);
            invoice.Total = invoice.Subtotal;
            dbContext.OrganizationInvoices.Add(invoice);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        TempData["BillingSuccess"] = billableSubscriptions.Length == 1
            ? "Invoice generated."
            : $"Generated {billableSubscriptions.Length} invoices, one per currency.";
        return RedirectToAction(nameof(Invoices), new { organizationId });
    }

    [HttpPost("organizations/{organizationId:guid}/billing/invoices/{invoiceId:guid}/payments")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordPayment(
        Guid organizationId,
        Guid invoiceId,
        RecordInvoicePaymentViewModel model,
        CancellationToken cancellationToken)
    {
        var authorization = await CheckManagerAccessAsync(organizationId, cancellationToken);
        if (authorization is not null)
            return authorization;

        if (!ModelState.IsValid)
        {
            TempData["BillingError"] = "Enter a valid payment amount.";
            return RedirectToAction(nameof(Invoices), new { organizationId });
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var invoice = await dbContext.OrganizationInvoices
            .FromSqlInterpolated(
                $"SELECT * FROM customer_management.organization_invoices WHERE id = {invoiceId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);
        if (invoice is null)
            return NotFound();

        var billingAccount = await dbContext.BillingAccounts
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == invoice.BillingAccountId &&
                    item.OrganizationId == organizationId,
                cancellationToken);
        if (billingAccount is null)
            return NotFound();
        if (invoice.Status is not ("Issued" or "PartiallyPaid"))
        {
            await transaction.RollbackAsync(cancellationToken);
            TempData["BillingError"] = "Only unpaid invoices can receive payments.";
            return RedirectToAction(nameof(Invoices), new { organizationId });
        }

        var totalPaid = await dbContext.InvoicePayments
            .Where(item => item.InvoiceId == invoiceId)
            .SumAsync(item => (decimal?)item.Amount, cancellationToken) ?? 0;
        var balance = invoice.Total - totalPaid;
        if (model.Amount <= 0 || model.Amount > balance || model.Amount != decimal.Round(model.Amount, 4))
        {
            await transaction.RollbackAsync(cancellationToken);
            TempData["BillingError"] =
                $"Payment must be greater than zero, no more than {invoice.CurrencyCode} {FormatAmount(balance)}, and use at most four decimal places.";
            return RedirectToAction(nameof(Invoices), new { organizationId });
        }

        var now = DateTimeOffset.UtcNow;
        dbContext.InvoicePayments.Add(new InvoicePayment
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            Amount = model.Amount,
            CurrencyCode = invoice.CurrencyCode,
            ExternalReference = CleanOptional(model.ExternalReference),
            RecordedByUserId = GetUserId(),
            RecordedAt = now
        });
        invoice.Status = model.Amount == balance ? "Paid" : "PartiallyPaid";

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        TempData["BillingSuccess"] = "External payment recorded.";
        return RedirectToAction(nameof(Invoices), new { organizationId });
    }

    private async Task<IActionResult?> CheckManagerAccessAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var role = await dbContext.OrganizationMemberships
            .AsNoTracking()
            .Where(item => item.OrganizationId == organizationId && item.UserId == GetUserId())
            .Select(item => item.Role)
            .SingleOrDefaultAsync(cancellationToken);
        if (role is null)
            return NotFound();
        return role is "Owner" or "Administrator" ? null : Forbid();
    }

    private string GetUserId() =>
        User.FindFirstValue("sub")
        ?? throw new InvalidOperationException("The authenticated identity has no subject claim.");

    private string GetUserEmail() =>
        User.FindFirstValue("email")
        ?? throw new InvalidOperationException("The authenticated identity has no email claim.");

    private static string? CleanOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string BuildBillingAddress(BillingAccount account) =>
        string.Join(", ", new[]
        {
            account.AddressLine1,
            account.AddressLine2,
            account.City,
            account.Region,
            account.PostalCode,
            account.CountryCode,
            account.TaxIdentifier is null ? null : $"Tax ID {account.TaxIdentifier}"
        }.Where(value => !string.IsNullOrWhiteSpace(value)));

    private static string FormatAmount(decimal amount) =>
        amount.ToString("0.####", CultureInfo.InvariantCulture);

    private static DateTimeOffset AddBillingInterval(DateTimeOffset periodStart, string interval) =>
        interval switch
        {
            "Monthly" => periodStart.AddMonths(1),
            "Yearly" => periodStart.AddYears(1),
            _ => throw new InvalidOperationException($"Unsupported billing interval '{interval}'.")
        };
}
