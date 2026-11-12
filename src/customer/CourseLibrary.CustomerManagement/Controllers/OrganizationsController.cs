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
        var subscription = new OrganizationSubscription
        {
            Id = Guid.NewGuid(),
            OrganizationId = organization.Id,
            ProductId = plan.ProductId,
            ProductPlanId = plan.Id,
            Status = "Selected",
            SelectedAt = now
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
        dbContext.OrganizationSubscriptions.Add(subscription);
        if (model.GiveCreatorProductAccess)
        {
            dbContext.OrganizationProductAccess.Add(new OrganizationProductAccess
            {
                OrganizationId = organization.Id,
                UserId = userId,
                ProductId = plan.ProductId,
                GrantedAt = now
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return RedirectToAction(nameof(Details), new { id = organization.Id });
    }

    [HttpGet("organizations/{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var membership = await FindMembershipAsync(id, GetUserId(), cancellationToken);
        if (membership is null)
            return NotFound();

        var organization = await dbContext.Organizations
            .AsNoTracking()
            .AsSplitQuery()
            .Include(item => item.Memberships)
                .ThenInclude(item => item.ProductAccess)
                    .ThenInclude(item => item.Product)
            .Include(item => item.Subscriptions)
                .ThenInclude(item => item.ProductPlan)
                    .ThenInclude(item => item!.Product)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (organization is null)
            return NotFound();

        var availableProductIds = organization.Subscriptions
            .Where(item => item.Status == "Selected")
            .Select(item => item.ProductId)
            .ToHashSet();
        return View(new OrganizationDetailsViewModel
        {
            Id = organization.Id,
            Name = organization.Name,
            CurrentRole = membership.Role,
            CanManage = CanManage(membership.Role),
            Members = organization.Memberships
                .OrderBy(item => item.Email)
                .Select(item => new OrganizationMemberItem(
                    item.UserId,
                    item.Email,
                    item.Role,
                    item.ProductAccess
                        .Where(access => availableProductIds.Contains(access.ProductId))
                        .Select(access => access.Product!.Name)
                        .OrderBy(name => name)
                        .ToList()))
                .ToList(),
            Subscriptions = organization.Subscriptions
                .OrderBy(item => item.ProductPlan!.Product!.Name)
                .Select(item => new OrganizationSubscriptionItem(
                    item.ProductPlan!.Product!.Name,
                    item.ProductPlan.Name,
                    item.Status))
                .ToList()
        });
    }

    [HttpGet("organizations/{id:guid}/products/add")]
    public async Task<IActionResult> AddProduct(Guid id, CancellationToken cancellationToken)
    {
        var authorization = await CheckManagerAccessAsync(id, cancellationToken);
        if (authorization is not null)
            return authorization;

        var model = new AddOrganizationProductViewModel { OrganizationId = id };
        await LoadAddProductOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost("organizations/{id:guid}/products/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddProduct(
        Guid id,
        AddOrganizationProductViewModel model,
        CancellationToken cancellationToken)
    {
        var authorization = await CheckManagerAccessAsync(id, cancellationToken);
        if (authorization is not null)
            return authorization;

        model.OrganizationId = id;
        await LoadAddProductOptionsAsync(model, cancellationToken);
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

        if (await dbContext.OrganizationSubscriptions.AnyAsync(
                item => item.OrganizationId == id && item.ProductId == plan.ProductId,
                cancellationToken))
        {
            ModelState.AddModelError(nameof(model.ProductPlanId), "This organization already has a subscription for that product.");
            return View(model);
        }

        var selectedUserIds = model.SelectedUserIds.Distinct(StringComparer.Ordinal).ToArray();
        var organizationUserIds = await dbContext.OrganizationMemberships
            .Where(item => item.OrganizationId == id)
            .Select(item => item.UserId)
            .ToListAsync(cancellationToken);
        if (selectedUserIds.Except(organizationUserIds, StringComparer.Ordinal).Any())
        {
            ModelState.AddModelError(nameof(model.SelectedUserIds), "Product access can only be assigned to organization members.");
            return View(model);
        }

        var now = DateTimeOffset.UtcNow;
        var subscription = new OrganizationSubscription
        {
            Id = Guid.NewGuid(),
            OrganizationId = id,
            ProductId = plan.ProductId,
            ProductPlanId = plan.Id,
            Status = "Selected",
            SelectedAt = now
        };
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        dbContext.OrganizationSubscriptions.Add(subscription);
        foreach (var userId in selectedUserIds)
        {
            dbContext.OrganizationProductAccess.Add(new OrganizationProductAccess
            {
                OrganizationId = id,
                UserId = userId,
                ProductId = plan.ProductId,
                GrantedAt = now
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet("organizations/{id:guid}/members/{userId}/products")]
    public async Task<IActionResult> ManageProductAccess(
        Guid id,
        string userId,
        CancellationToken cancellationToken)
    {
        var authorization = await CheckManagerAccessAsync(id, cancellationToken);
        if (authorization is not null)
            return authorization;

        var member = await dbContext.OrganizationMemberships
            .AsNoTracking()
            .Include(item => item.ProductAccess)
            .SingleOrDefaultAsync(
                item => item.OrganizationId == id && item.UserId == userId,
                cancellationToken);
        if (member is null)
            return NotFound();

        var model = new ManageMemberProductAccessViewModel
        {
            OrganizationId = id,
            UserId = member.UserId,
            Email = member.Email,
            Role = member.Role,
            SelectedProductIds = member.ProductAccess.Select(item => item.ProductId).ToList()
        };
        await LoadMemberProductOptionsAsync(model, cancellationToken);
        model.SelectedProductIds = model.SelectedProductIds
            .Intersect(model.AvailableProducts.Select(item => item.ProductId))
            .ToList();
        return View(model);
    }

    [HttpPost("organizations/{id:guid}/members/{userId}/products")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManageProductAccess(
        Guid id,
        string userId,
        ManageMemberProductAccessViewModel model,
        CancellationToken cancellationToken)
    {
        var authorization = await CheckManagerAccessAsync(id, cancellationToken);
        if (authorization is not null)
            return authorization;

        var member = await dbContext.OrganizationMemberships
            .Include(item => item.ProductAccess)
            .SingleOrDefaultAsync(
                item => item.OrganizationId == id && item.UserId == userId,
                cancellationToken);
        if (member is null)
            return NotFound();

        model.OrganizationId = id;
        model.UserId = userId;
        model.Email = member.Email;
        model.Role = member.Role;
        await LoadMemberProductOptionsAsync(model, cancellationToken);
        var allowedProductIds = model.AvailableProducts.Select(item => item.ProductId).ToHashSet();
        var selectedProductIds = model.SelectedProductIds.Distinct().ToHashSet();
        if (selectedProductIds.Except(allowedProductIds).Any())
        {
            ModelState.AddModelError(
                nameof(model.SelectedProductIds),
                "Only products currently subscribed to by this organization can be assigned.");
        }
        if (!ModelState.IsValid)
            return View(model);

        var currentAccess = member.ProductAccess.ToList();
        dbContext.OrganizationProductAccess.RemoveRange(
            currentAccess.Where(item => !selectedProductIds.Contains(item.ProductId)));
        var currentProductIds = currentAccess.Select(item => item.ProductId).ToHashSet();
        foreach (var productId in selectedProductIds.Except(currentProductIds))
        {
            dbContext.OrganizationProductAccess.Add(new OrganizationProductAccess
            {
                OrganizationId = id,
                UserId = userId,
                ProductId = productId,
                GrantedAt = DateTimeOffset.UtcNow
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
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

    private async Task LoadAddProductOptionsAsync(
        AddOrganizationProductViewModel model,
        CancellationToken cancellationToken)
    {
        model.ProductPlans = await dbContext.ProductPlans
            .AsNoTracking()
            .Where(item => item.IsActive &&
                item.Product!.IsActive &&
                !dbContext.OrganizationSubscriptions.Any(subscription =>
                    subscription.OrganizationId == model.OrganizationId &&
                    subscription.ProductId == item.ProductId))
            .OrderBy(item => item.Product!.Name)
            .ThenBy(item => item.Name)
            .Select(item => new ProductPlanOption(item.Id, item.Product!.Name, item.Name))
            .ToListAsync(cancellationToken);
        model.Members = await dbContext.OrganizationMemberships
            .AsNoTracking()
            .Where(item => item.OrganizationId == model.OrganizationId)
            .OrderBy(item => item.Email)
            .Select(item => new OrganizationMemberOption(item.UserId, item.Email))
            .ToListAsync(cancellationToken);
    }

    private async Task LoadMemberProductOptionsAsync(
        ManageMemberProductAccessViewModel model,
        CancellationToken cancellationToken)
    {
        model.AvailableProducts = await dbContext.OrganizationSubscriptions
            .AsNoTracking()
            .Where(item => item.OrganizationId == model.OrganizationId &&
                item.Status == "Selected" &&
                item.ProductPlan!.IsActive &&
                item.ProductPlan.Product!.IsActive)
            .OrderBy(item => item.ProductPlan!.Product!.Name)
            .Select(item => new ProductAccessOption(
                item.ProductId,
                item.ProductPlan!.Product!.Name,
                item.ProductPlan.Name))
            .ToListAsync(cancellationToken);
    }

    private async Task<IActionResult?> CheckManagerAccessAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var membership = await FindMembershipAsync(organizationId, GetUserId(), cancellationToken);
        if (membership is null)
            return NotFound();
        return CanManage(membership.Role) ? null : Forbid();
    }

    private Task<OrganizationMembership?> FindMembershipAsync(
        Guid organizationId,
        string userId,
        CancellationToken cancellationToken) =>
        dbContext.OrganizationMemberships
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.OrganizationId == organizationId && item.UserId == userId,
                cancellationToken);

    private string GetUserId() =>
        User.FindFirstValue("sub")
        ?? throw new InvalidOperationException("The authenticated identity has no subject claim.");

    private string GetUserEmail() =>
        User.FindFirstValue("email")
        ?? throw new InvalidOperationException("The authenticated identity has no email claim.");

    private static bool CanManage(string role) => role is "Owner" or "Administrator";
    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();
}
