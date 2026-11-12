using CourseLibrary.CustomerManagement.Data;
using CourseLibrary.CustomerManagement.Domain.Entities;
using CourseLibrary.CustomerManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CourseLibrary.CustomerManagement.Controllers;

[Authorize]
public sealed class OrganizationsController(CustomerManagementDbContext dbContext) : Controller
{
    [HttpGet("/")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var organizations = await dbContext.OrganizationMemberships
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.Organization!.Name)
            .Select(item => new OrganizationListItem(
                item.OrganizationId,
                item.Organization!.Name,
                item.Role,
                item.Organization.Memberships.Count))
            .ToListAsync(cancellationToken);

        return View(organizations);
    }

    [HttpGet("organizations/create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new CreateOrganizationViewModel();
        await LoadProductPlansAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost("organizations/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateOrganizationViewModel model,
        CancellationToken cancellationToken)
    {
        await LoadProductPlansAsync(model, cancellationToken);
        if (!ModelState.IsValid)
            return View(model);

        var plan = await dbContext.ProductPlans
            .Include(item => item.Product)
            .SingleOrDefaultAsync(
                item => item.Id == model.ProductPlanId &&
                    item.IsActive &&
                    item.Product!.IsActive,
                cancellationToken);
        if (plan is null)
        {
            ModelState.AddModelError(nameof(model.ProductPlanId), "Select an available product plan.");
            return View(model);
        }

        var userId = GetUserId();
        var email = GetUserEmail();
        var now = DateTimeOffset.UtcNow;
        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = model.Name.Trim(),
            CreatedByUserId = userId,
            CreatedAt = now
        };

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        dbContext.Organizations.Add(organization);
        dbContext.OrganizationMemberships.Add(new OrganizationMembership
        {
            OrganizationId = organization.Id,
            UserId = userId,
            Email = email,
            NormalizedEmail = NormalizeEmail(email),
            Role = "Owner",
            JoinedAt = now
        });
        dbContext.OrganizationSubscriptions.Add(new OrganizationSubscription
        {
            Id = Guid.NewGuid(),
            OrganizationId = organization.Id,
            ProductPlanId = plan.Id,
            Status = "Selected",
            SelectedAt = now
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return RedirectToAction(nameof(Details), new { id = organization.Id });
    }

    [HttpGet("organizations/{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var membership = await dbContext.OrganizationMemberships
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.OrganizationId == id && item.UserId == userId,
                cancellationToken);
        if (membership is null)
            return NotFound();

        var organization = await dbContext.Organizations
            .AsNoTracking()
            .AsSplitQuery()
            .Where(item => item.Id == id)
            .Select(item => new
            {
                item.Id,
                item.Name,
                Members = item.Memberships
                    .OrderBy(member => member.Email)
                    .Select(member => new OrganizationMemberItem(member.Email, member.Role))
                    .ToList(),
                Subscriptions = item.Subscriptions
                    .Select(subscription => new OrganizationSubscriptionItem(
                        subscription.ProductPlan!.Product!.Name,
                        subscription.ProductPlan.Name,
                        subscription.Status))
                    .ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (organization is null)
            return NotFound();

        return View(new OrganizationDetailsViewModel
        {
            Id = organization.Id,
            Name = organization.Name,
            CurrentRole = membership.Role,
            Members = organization.Members,
            Subscriptions = organization.Subscriptions
        });
    }

    private async Task LoadProductPlansAsync(
        CreateOrganizationViewModel model,
        CancellationToken cancellationToken)
    {
        model.ProductPlans = await dbContext.ProductPlans
            .AsNoTracking()
            .Where(item => item.IsActive && item.Product!.IsActive)
            .OrderBy(item => item.Product!.Name)
            .ThenBy(item => item.Name)
            .Select(item => new ProductPlanOption(item.Id, item.Product!.Name, item.Name))
            .ToListAsync(cancellationToken);
    }

    private string GetUserId() =>
        User.FindFirstValue("sub")
        ?? throw new InvalidOperationException("The authenticated identity has no subject claim.");

    private string GetUserEmail() =>
        User.FindFirstValue("email")
        ?? throw new InvalidOperationException("The authenticated identity has no email claim.");

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();
}
