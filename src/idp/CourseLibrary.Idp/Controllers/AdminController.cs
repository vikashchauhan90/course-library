using CourseLibrary.Idp.Domain.Entities;
using CourseLibrary.Idp.Domain.Authorization;
using CourseLibrary.Idp.Models.Admin;
using CourseLibrary.Idp.Models;
using CourseLibrary.Idp.Persistence.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace CourseLibrary.Idp.Controllers;

[Authorize(Policy = "AdminMfa")]
[Route("admin")]
public sealed class AdminController(
    UserManager<ApplicationUser> userManager,
    IOpenIddictApplicationManager applicationManager,
    IOpenIddictScopeManager scopeManager,
    ApplicationDbContext dbContext,
    ILogger<AdminController> logger,
    RoleManager<ApplicationRole> roleManager,
    CourseLibrary.Idp.Abstractions.IEmailSender emailSender,
    IConfiguration configuration) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        return View(new AdminOverviewViewModel(
            await dbContext.Users.IgnoreQueryFilters().CountAsync(),
            await roleManager.Roles.CountAsync(),
            await dbContext.OpenIddictApplications.CountAsync(),
            await dbContext.OpenIddictScopes.CountAsync()));
    }

    [HttpGet("users")]
    public async Task<IActionResult> Users(string status = "all", int page = 1)
    {
        status = status is "active" or "inactive" ? status : "all";
        page = Math.Max(page, 1);
        const int pageSize = 50;
        var now = DateTimeOffset.UtcNow;
        var allUsers = dbContext.Users.IgnoreQueryFilters();
        var activeUsers = allUsers.Where(user =>
            user.DeletedAt == null &&
            user.EmailConfirmed &&
            (user.LockoutEnd == null || user.LockoutEnd <= now));
        var activeCount = await activeUsers.CountAsync();
        var inactiveCount = await allUsers.CountAsync() - activeCount;
        var filteredUsers = status switch
        {
            "active" => activeUsers,
            "inactive" => allUsers.Where(user =>
                user.DeletedAt != null ||
                !user.EmailConfirmed ||
                (user.LockoutEnd != null && user.LockoutEnd > now)),
            _ => allUsers
        };
        var totalCount = status switch
        {
            "active" => activeCount,
            "inactive" => inactiveCount,
            _ => activeCount + inactiveCount
        };
        page = Math.Min(page, Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize)));
        var users = await filteredUsers
            .OrderBy(user => user.UserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        var items = new List<AdminUserItem>(users.Count);
        foreach (var user in users)
        {
            var isLockedOut = user.LockoutEnd > now;
            var userStatus = user.DeletedAt is not null
                ? "Deactivated"
                : !user.EmailConfirmed
                    ? "Pending activation"
                    : isLockedOut
                        ? "Locked"
                        : "Active";
            items.Add(new(
                user.Id,
                user.UserName ?? user.Id,
                user.Email ?? string.Empty,
                userStatus,
                isLockedOut,
                user.DeletedAt is not null,
                user.EmailConfirmed && user.DeletedAt is null,
                await userManager.IsInRoleAsync(user, "Administrator"),
                user.LastLoginAt,
                user.PasswordChangedAt));
        }

        return View(new AdminUsersViewModel(items, status, page, pageSize, totalCount, activeCount, inactiveCount));
    }

    [HttpGet("invitations")]
    public async Task<IActionResult> Invitations()
    {
        var now = DateTimeOffset.UtcNow;
        var invitations = await dbContext.UserInvitations
            .AsNoTracking()
            .Join(
                dbContext.Users.IgnoreQueryFilters(),
                invitation => invitation.UserId,
                user => user.Id,
                (invitation, user) => new { invitation, user })
            .OrderByDescending(item => item.invitation.CreatedAt)
            .Select(item => new
            {
                item.invitation.Id,
                Email = item.user.Email ?? string.Empty,
                item.user.FullName,
                item.invitation.CreatedAt,
                item.invitation.ExpiresAt,
                item.invitation.AcceptedAt,
                item.invitation.RevokedAt,
                IsDeactivated = item.user.DeletedAt != null
            })
            .ToListAsync();

        var items = invitations.Select(invitation =>
        {
            var isExpired = invitation.ExpiresAt <= now;
            var status = invitation.AcceptedAt is not null
                ? "Accepted"
                : invitation.RevokedAt is not null
                    ? "Revoked"
                    : isExpired
                        ? "Expired"
                        : "Pending";
            var canResend = invitation.AcceptedAt is null && !invitation.IsDeactivated;
            var canRevoke = status == "Pending";
            return new AdminInvitationItem(
                invitation.Id,
                invitation.Email,
                invitation.FullName,
                invitation.CreatedAt,
                invitation.ExpiresAt,
                invitation.AcceptedAt,
                invitation.RevokedAt,
                status,
                canResend,
                canRevoke);
        }).ToList();

        return View(new AdminInvitationsViewModel(items));
    }

    [HttpGet("roles")]
    public async Task<IActionResult> Roles()
    {
        var roles = new List<AdminRoleItem>();
        foreach (var role in await roleManager.Roles.OrderBy(x => x.Name).ToListAsync())
        {
            var permissions = await dbContext.RolePermissions
                .Where(assignment => assignment.RoleId == role.Id)
                .Select(assignment => assignment.PermissionId)
                .ToHashSetAsync();
            roles.Add(new(role.Name ?? string.Empty, permissions));
        }
        var permissionsCatalog = PermissionCatalog.Definitions
            .Select(permission => new AdminPermissionItem(permission.Code, permission.DisplayName))
            .ToList();
        return View(new AdminRolesViewModel(roles, permissionsCatalog));
    }

    [HttpGet("clients")]
    public async Task<IActionResult> Clients() => View(new AdminClientsViewModel(await GetClientItemsAsync()));

    [HttpGet("scopes")]
    public async Task<IActionResult> Scopes() => View(new AdminScopesViewModel(await GetScopeItemsAsync()));

    private async Task<IReadOnlyList<AdminClientItem>> GetClientItemsAsync()
    {
        var now = DateTimeOffset.UtcNow;
        var applications = await dbContext.OpenIddictApplications
            .AsNoTracking()
            .Where(application => application.DeletedAt == null)
            .OrderBy(application => application.ClientId)
            .ToListAsync();

        var items = new List<AdminClientItem>(applications.Count);
        await foreach (var client in applicationManager.ListAsync())
        {
            var clientId = await applicationManager.GetClientIdAsync(client) ?? string.Empty;
            var application = applications.SingleOrDefault(item => item.ClientId == clientId);
            if (application is null)
                continue;

            var permissions = await applicationManager.GetPermissionsAsync(client);
            var scopes = permissions
                .Where(permission => permission.StartsWith(
                    Permissions.Prefixes.Scope,
                    StringComparison.Ordinal))
                .Select(permission => permission[Permissions.Prefixes.Scope.Length..])
                .OrderBy(scope => scope)
                .ToList();

            var secretExpiry = ClientSecretLifecycle.GetEffectiveExpiry(application, configuration);
            items.Add(new AdminClientItem(
                clientId,
                application.DisplayName,
                application.CreatedAt,
                application.ClientSecret is null ? null : application.SecretCreatedAt ?? application.CreatedAt,
                application.SecretRotatedAt,
                secretExpiry,
                secretExpiry is not null && secretExpiry <= now,
                application.ClientSecret is not null,
                string.Join(", ", scopes)));
        }

        return items.OrderBy(item => item.ClientId).ToList();
    }

    private async Task<IReadOnlyList<AdminScopeItem>> GetScopeItemsAsync()
    {
        var scopes = new List<AdminScopeItem>();
        await foreach (var scope in scopeManager.ListAsync())
        {
            var resources = await scopeManager.GetResourcesAsync(scope);
            scopes.Add(new(
                await scopeManager.GetNameAsync(scope) ?? string.Empty,
                await scopeManager.GetDisplayNameAsync(scope),
                string.Join(", ", resources)));
        }
        return scopes.OrderBy(scope => scope.Name).ToList();
    }

    private DateTimeOffset GetSecretExpiry(DateTimeOffset createdAt)
        => ClientSecretLifecycle.GetExpiry(createdAt, configuration);

    [HttpGet("users/create")]
    public IActionResult CreateUser() => View(new CreateUserViewModel());

    [HttpGet("users/invite")]
    public IActionResult InviteUser() => View(new InviteUserViewModel());

    [HttpPost("users/invite")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InviteUser(InviteUserViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (await IsEmailInUseAsync(model.Email))
        { ModelState.AddModelError(nameof(model.Email), "A user with this email already exists."); return View(model); }

        var now = DateTimeOffset.UtcNow;
        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName, CreatedAt = now, LockoutEnabled = true };
        var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        await using (var transaction = await dbContext.Database.BeginTransactionAsync())
        {
            var result = await userManager.CreateAsync(user);
            if (!result.Succeeded) { AddErrors(result); return View(model); }
            if (model.IsAdministrator)
            {
                result = await userManager.AddToRoleAsync(user, "Administrator");
                if (!result.Succeeded) { AddErrors(result); return View(model); }
            }

            var activeInvitations = await dbContext.UserInvitations
                .Where(x => x.UserId == user.Id && x.AcceptedAt == null && x.RevokedAt == null && x.ExpiresAt > now)
                .ToListAsync();
            foreach (var invitation in activeInvitations) invitation.RevokedAt = now;
            dbContext.UserInvitations.Add(new()
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.Id,
                TokenHash = HashToken(token),
                CreatedAt = now,
                ExpiresAt = now.AddHours(Math.Clamp(configuration.GetValue("Security:InvitationLifetimeHours", 72), 1, 720))
            });
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        var link = Url.Action("AcceptInvitation", "Invitation", new { email = user.Email, token }, Request.Scheme)
            ?? throw new InvalidOperationException("Could not create invitation link.");
        await emailSender.SendAsync(user.Email!, "Your Course Library invitation", link);
        logger.LogInformation("Administrator {AdministratorId} invited user {UserId}.", userManager.GetUserId(User), user.Id);
        TempData["InvitationLink"] = link;
        TempData["Success"] = "Invitation created and emailed. The link is single-use and expires at the configured time.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("users/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (await IsEmailInUseAsync(model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "A user with this email already exists.");
            return View(model);
        }

        var now = DateTimeOffset.UtcNow;
        var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, FullName = model.FullName, EmailConfirmed = true, CreatedAt = now, PasswordChangedAt = now };
        await using (var transaction = await dbContext.Database.BeginTransactionAsync())
        {
            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded) { AddErrors(result); return View(model); }
            if (model.IsAdministrator)
            {
                result = await userManager.AddToRoleAsync(user, "Administrator");
                if (!result.Succeeded) { AddErrors(result); return View(model); }
            }
            await transaction.CommitAsync();
        }
        logger.LogInformation("Administrator {AdministratorId} created user {UserId}.", userManager.GetUserId(User), user.Id);
        TempData["Success"] = "User created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("users/{id}/lock")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserLock(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();
        if (user.Id == userManager.GetUserId(User)) { TempData["Error"] = "You cannot lock your own account."; return RedirectToAction(nameof(Index)); }
        user.LockoutEnabled = true;
        user.LockoutEnd = user.LockoutEnd > DateTimeOffset.UtcNow ? null : DateTimeOffset.MaxValue;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded) AddErrors(result);
        else logger.LogInformation("Administrator {AdministratorId} changed lock state for user {UserId}.", userManager.GetUserId(User), user.Id);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("users/{id}/edit")]
    public async Task<IActionResult> EditUser(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();
        return View(new EditUserViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            IsAdministrator = await userManager.IsInRoleAsync(user, PermissionCatalog.AdministratorRole),
            IsLocked = user.LockoutEnd > DateTimeOffset.UtcNow,
            AvailableRoles = await roleManager.Roles
                .Where(role => role.Name != null)
                .OrderBy(role => role.Name)
                .Select(role => role.Name!)
                .ToListAsync(),
            SelectedRoles = (await userManager.GetRolesAsync(user)).ToList()
        });
    }

    [HttpPost("users/{id}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(string id, EditUserViewModel model)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();
        if (!ModelState.IsValid) return View(model);
        user.FullName = model.FullName; user.Email = model.Email; user.UserName = model.Email;
        var result = await userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            var currentRoles = await userManager.GetRolesAsync(user);
            var availableRoles = await roleManager.Roles
                .Where(role => role.Name != null)
                .Select(role => role.Name!)
                .ToListAsync();
            var selectedRoles = model.SelectedRoles
                .Intersect(availableRoles, StringComparer.OrdinalIgnoreCase)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (user.Id == userManager.GetUserId(User))
                selectedRoles.Add(PermissionCatalog.AdministratorRole);

            var rolesToRemove = currentRoles
                .Where(role => !selectedRoles.Contains(role))
                .ToList();
            var rolesToAdd = selectedRoles
                .Where(role => !currentRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (rolesToRemove.Count > 0)
                result = await userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (result.Succeeded && rolesToAdd.Count > 0)
                result = await userManager.AddToRolesAsync(user, rolesToAdd);
        }
        if (!result.Succeeded) { AddErrors(result); return View(model); }
        await userManager.SetLockoutEndDateAsync(user, model.IsLocked ? DateTimeOffset.MaxValue : null);
        logger.LogInformation("Administrator {AdministratorId} edited user {UserId}.", userManager.GetUserId(User), user.Id);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("roles/create")]
    public IActionResult CreateRole() => View(new CreateRoleViewModel());

    [HttpPost("roles/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var normalizedName = model.Name.Trim();
        if (await roleManager.RoleExistsAsync(normalizedName))
        {
            ModelState.AddModelError(nameof(model.Name), "That role already exists.");
            return View(model);
        }

        var result = await roleManager.CreateAsync(new ApplicationRole
        {
            Name = normalizedName,
            NormalizedName = normalizedName.ToUpperInvariant(),
            CreatedAt = DateTimeOffset.UtcNow
        });
        if (!result.Succeeded)
        {
            AddErrors(result);
            return View(model);
        }

        logger.LogInformation(
            "Administrator {AdministratorId} created role {RoleName}.",
            userManager.GetUserId(User),
            normalizedName);
        TempData["Success"] = "Role created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("users/{id}/reset-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendUserPasswordReset(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();
        if (!user.EmailConfirmed || string.IsNullOrWhiteSpace(user.Email))
        {
            TempData["Error"] = "A password-reset link can only be sent to a confirmed account with an email address.";
            return RedirectToAction(nameof(Users));
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var link = Url.Action("ResetPassword", "Account", new { email = user.Email, token }, Request.Scheme)
            ?? throw new InvalidOperationException("Could not create password-reset link.");
        await emailSender.SendAsync(user.Email, "Reset your password", link);
        logger.LogInformation("Administrator {AdministratorId} sent a password-reset link to user {UserId}.", userManager.GetUserId(User), id);
        TempData["Success"] = "Password-reset link sent.";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost("users/{id}/revoke-sessions")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeUserSessions(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();
        await userManager.UpdateSecurityStampAsync(user);
        await dbContext.OpenIddictTokens.Where(x => x.Subject == id && x.Status == Statuses.Valid).ExecuteUpdateAsync(x => x.SetProperty(t => t.Status, Statuses.Revoked));
        logger.LogInformation("Administrator {AdministratorId} revoked sessions for user {UserId}.", userManager.GetUserId(User), id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("invitations/{id}/revoke")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeInvitation(string id)
    {
        var invitation = await dbContext.UserInvitations.SingleOrDefaultAsync(item => item.Id == id);
        if (invitation is null) return NotFound();
        if (invitation.AcceptedAt is not null || invitation.RevokedAt is not null || invitation.ExpiresAt <= DateTimeOffset.UtcNow)
            return BadRequest("Only pending invitations can be revoked.");
        invitation.RevokedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Invitations));
    }

    [HttpPost("invitations/{id}/resend")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendInvitation(string id)
    {
        var invitation = await dbContext.UserInvitations.SingleOrDefaultAsync(x => x.Id == id);
        if (invitation is null) return NotFound();
        var user = await userManager.FindByIdAsync(invitation.UserId);
        if (user is null || user.Email is null) return NotFound();
        if (invitation.AcceptedAt is not null || user.EmailConfirmed)
            return BadRequest("An accepted invitation cannot be resent.");

        var now = DateTimeOffset.UtcNow;
        var activeInvitations = await dbContext.UserInvitations
            .Where(item => item.UserId == user.Id &&
                item.AcceptedAt == null &&
                item.RevokedAt == null &&
                item.ExpiresAt > now)
            .ToListAsync();
        foreach (var activeInvitation in activeInvitations)
            activeInvitation.RevokedAt = now;

        var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        dbContext.UserInvitations.Add(new UserInvitation
        {
            Id = Guid.NewGuid().ToString(),
            UserId = user.Id,
            TokenHash = HashToken(token),
            CreatedAt = now,
            ExpiresAt = now.AddHours(Math.Clamp(configuration.GetValue("Security:InvitationLifetimeHours", 72), 1, 720))
        });
        await dbContext.SaveChangesAsync();
        var link = Url.Action("AcceptInvitation", "Invitation", new { email = user.Email, token }, Request.Scheme)!;
        await emailSender.SendAsync(user.Email, "Your Course Library invitation", link);
        TempData["Success"] = "A new invitation was sent. Previous pending invitations were revoked.";
        return RedirectToAction(nameof(Invitations));
    }

    [HttpGet("clients/create")]
    public async Task<IActionResult> CreateClient() => View(await BuildCreateClientModelAsync());

    [HttpPost("clients/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateClient(CreateClientViewModel model)
    {
        model.ClientId = model.IsMachineClient
            ? CreateMachineClientId()
            : model.ClientId?.Trim();
        model.AllowClientCredentials = model.IsMachineClient || model.AllowClientCredentials;

        Uri? redirectUri = null;
        if (!model.IsMachineClient && string.IsNullOrWhiteSpace(model.ClientId))
            ModelState.AddModelError(nameof(model.ClientId), "A client ID is required for browser applications.");
        if (model.IsMachineClient && model.AllowAuthorizationCode)
            ModelState.AddModelError(string.Empty, "Machine clients use client credentials only.");
        if (!model.AllowClientCredentials && !model.AllowAuthorizationCode)
            ModelState.AddModelError(string.Empty, "Select at least one grant type.");
        if (model.AllowAuthorizationCode && !Uri.TryCreate(model.RedirectUri, UriKind.Absolute, out redirectUri))
            ModelState.AddModelError(nameof(model.RedirectUri), "An absolute redirect URI is required for authorization code flow.");
        var validScopes = await GetScopeNamesAsync();
        if (model.AllowedScopes.Any(scope => !validScopes.Contains(scope, StringComparer.OrdinalIgnoreCase)))
            ModelState.AddModelError(nameof(model.AllowedScopes), "One or more selected scopes are not registered.");
        if (model.IsMachineClient && model.AllowedScopes.Count == 0)
            ModelState.AddModelError(nameof(model.AllowedScopes), "Select at least one audience scope for a machine client.");
        if (!ModelState.IsValid) { model.AvailableScopes = await GetScopeOptionsAsync(); return View(model); }
        if (await applicationManager.FindByClientIdAsync(model.ClientId!) is not null)
        { ModelState.AddModelError(nameof(model.ClientId), "That client ID already exists."); return View(model); }

        var secret = CreateSecret();
        var descriptor = new OpenIddictApplicationDescriptor { ClientId = model.ClientId, ClientSecret = secret, DisplayName = model.DisplayName };
        descriptor.Permissions.Add(Permissions.Endpoints.Token);
        foreach (var scope in model.AllowedScopes.Distinct(StringComparer.OrdinalIgnoreCase))
            descriptor.Permissions.Add(Permissions.Prefixes.Scope + scope);
        if (model.AllowClientCredentials) descriptor.Permissions.Add(Permissions.GrantTypes.ClientCredentials);
        if (model.AllowAuthorizationCode)
        {
            descriptor.RedirectUris.Add(redirectUri!);
            descriptor.Permissions.Add(Permissions.Endpoints.Authorization);
            descriptor.Permissions.Add(Permissions.GrantTypes.AuthorizationCode);
            descriptor.Permissions.Add(Permissions.ResponseTypes.Code);
            descriptor.Requirements.Add(Requirements.Features.ProofKeyForCodeExchange);
        }
        var createdAt = DateTimeOffset.UtcNow;
        await using (var transaction = await dbContext.Database.BeginTransactionAsync())
        {
            await applicationManager.CreateAsync(descriptor);
            var createdApplication = await dbContext.OpenIddictApplications
                .SingleAsync(application => application.ClientId == model.ClientId);
            createdApplication.SecretCreatedAt = createdAt;
            createdApplication.SecretExpiresAt = GetSecretExpiry(createdAt);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        logger.LogInformation("Administrator {AdministratorId} created OAuth client {ClientId}.", userManager.GetUserId(User), model.ClientId);
        TempData["ClientSecret"] = secret;
        TempData["Success"] = "Client created. Copy its secret now; it will not be displayed again.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<CreateClientViewModel> BuildCreateClientModelAsync()
    {
        return new CreateClientViewModel
        {
            AvailableScopes = await GetScopeOptionsAsync()
        };
    }

    private async Task<IReadOnlyList<AdminScopeOption>> GetScopeOptionsAsync()
    {
        var options = new List<AdminScopeOption>();
        await foreach (var scope in scopeManager.ListAsync())
        {
            options.Add(new AdminScopeOption(
                await scopeManager.GetNameAsync(scope) ?? string.Empty,
                await scopeManager.GetDisplayNameAsync(scope),
                string.Join(", ", await scopeManager.GetResourcesAsync(scope))));
        }
        return options.OrderBy(option => option.Name).ToList();
    }

    private async Task<IReadOnlySet<string>> GetScopeNamesAsync()
    {
        return (await GetScopeOptionsAsync())
            .Select(option => option.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static string CreateMachineClientId() =>
        $"m2m_{Convert.ToHexString(RandomNumberGenerator.GetBytes(12)).ToLowerInvariant()}";

    [HttpPost("clients/{clientId}/rotate-secret")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RotateClientSecret(string clientId)
    {
        var client = await applicationManager.FindByClientIdAsync(clientId);
        if (client is null) return NotFound();
        var descriptor = new OpenIddictApplicationDescriptor();
        await applicationManager.PopulateAsync(descriptor, client);
        if (string.IsNullOrEmpty(descriptor.ClientSecret))
            return BadRequest("This client has no secret to rotate.");

        var secret = CreateSecret();
        descriptor.ClientSecret = secret;
        var rotatedAt = DateTimeOffset.UtcNow;
        await using (var transaction = await dbContext.Database.BeginTransactionAsync())
        {
            await applicationManager.UpdateAsync(client, descriptor);
            var application = await dbContext.OpenIddictApplications
                .SingleAsync(item => item.ClientId == clientId);
            application.SecretCreatedAt ??= application.CreatedAt;
            application.SecretRotatedAt = rotatedAt;
            application.SecretExpiresAt = GetSecretExpiry(rotatedAt);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        logger.LogInformation("Administrator {AdministratorId} rotated the secret for OAuth client {ClientId}.", userManager.GetUserId(User), clientId);
        TempData["ClientSecret"] = secret;
        TempData["Success"] = "Client secret rotated. Copy the new value now; it will not be displayed again.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("clients/{clientId}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteClient(string clientId)
    {
        var client = await applicationManager.FindByClientIdAsync(clientId);
        if (client is null) return NotFound();
        await applicationManager.DeleteAsync(client);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("clients/{clientId}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditClient(string clientId, string displayName, string redirectUri)
    {
        var client = await applicationManager.FindByClientIdAsync(clientId);
        if (client is null) return NotFound();
        var descriptor = new OpenIddictApplicationDescriptor(); await applicationManager.PopulateAsync(descriptor, client);
        descriptor.DisplayName = displayName;
        descriptor.RedirectUris.Clear();
        if (Uri.TryCreate(redirectUri, UriKind.Absolute, out var uri)) descriptor.RedirectUris.Add(uri);
        await applicationManager.UpdateAsync(client, descriptor);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("scopes/{name}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteScope(string name)
    {
        var scope = await scopeManager.FindByNameAsync(name);
        if (scope is null) return NotFound();
        await scopeManager.DeleteAsync(scope);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("scopes/{name}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditScope(string name, string displayName, string resource)
    {
        var scope = await scopeManager.FindByNameAsync(name);
        if (scope is null) return NotFound();
        var descriptor = new OpenIddictScopeDescriptor(); await scopeManager.PopulateAsync(descriptor, scope);
        descriptor.DisplayName = displayName; descriptor.Resources.Clear(); descriptor.Resources.Add(resource);
        await scopeManager.UpdateAsync(scope, descriptor);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("roles/{roleName}/permissions")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRolePermission(string roleName, string permission, bool enabled)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role is null) return NotFound();

        if (!PermissionCatalog.TryNormalize(permission, out var normalizedPermission))
        {
            TempData["Error"] = "Unknown permission.";
            return RedirectToAction(nameof(Index));
        }

        var dbPermission = await dbContext.Permissions.FindAsync(normalizedPermission);
        if (dbPermission is null)
        {
            TempData["Error"] = "Permission is not configured.";
            return RedirectToAction(nameof(Index));
        }

        var assignment = await dbContext.RolePermissions.FindAsync(role.Id, dbPermission.Id);
        if (enabled && assignment is null)
        {
            dbContext.RolePermissions.Add(new RolePermission
            {
                RoleId = role.Id,
                PermissionId = dbPermission.Id
            });
        }
        else if (!enabled && assignment is not null)
        {
            dbContext.RolePermissions.Remove(assignment);
        }
        else return RedirectToAction(nameof(Index));

        await dbContext.SaveChangesAsync();

        logger.LogInformation(
            "Administrator {AdministratorId} changed permission {Permission} for role {RoleName} to {Enabled}.",
            userManager.GetUserId(User),
            normalizedPermission,
            roleName,
            enabled);
        return RedirectToAction(nameof(Index));
    }

    private async Task<AdminIndexViewModel> BuildIndexViewModelAsync()
    {
        var users = await userManager.Users.OrderBy(x => x.UserName).Take(100).ToListAsync();
        var userItems = new List<AdminUserItem>(users.Count);
        foreach (var user in users)
        {
            var isLockedOut = user.LockoutEnd > DateTimeOffset.UtcNow;
            userItems.Add(new(
                user.Id,
                user.UserName ?? user.Id,
                user.Email ?? string.Empty,
                !user.EmailConfirmed ? "Pending activation" : isLockedOut ? "Locked" : "Active",
                isLockedOut,
                user.DeletedAt is not null,
                user.DeletedAt is null && user.EmailConfirmed,
                await userManager.IsInRoleAsync(user, PermissionCatalog.AdministratorRole),
                user.LastLoginAt,
                user.PasswordChangedAt));
        }

        var roles = new List<AdminRoleItem>();
        foreach (var role in await roleManager.Roles.OrderBy(x => x.Name).ToListAsync())
        {
            var rolePermissions = await dbContext.RolePermissions
                .Where(assignment => assignment.RoleId == role.Id)
                .Select(assignment => assignment.PermissionId)
                .ToHashSetAsync();
            roles.Add(new AdminRoleItem(
                role.Name ?? string.Empty,
                rolePermissions));
        }

        return new AdminIndexViewModel
        {
            Users = userItems,
            Roles = roles,
            Permissions = PermissionCatalog.Definitions
                .Select(permission => new AdminPermissionItem(permission.Code, permission.DisplayName))
                .ToList(),
            Clients = [],
            Scopes = []
        };
    }

    [HttpGet("scopes/create")]
    public IActionResult CreateScope() => View(new CreateScopeViewModel());

    [HttpPost("scopes/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateScope(CreateScopeViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (await scopeManager.FindByNameAsync(model.Name) is not null)
        { ModelState.AddModelError(nameof(model.Name), "That scope already exists."); return View(model); }
        await scopeManager.CreateAsync(new OpenIddictScopeDescriptor { Name = model.Name, DisplayName = model.DisplayName, Resources = { model.Resource } });
        logger.LogInformation("Administrator {AdministratorId} created OAuth scope {Scope}.", userManager.GetUserId(User), model.Name);
        TempData["Success"] = "Scope created.";
        return RedirectToAction(nameof(Index));
    }

    private void AddErrors(IdentityResult result) { foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description); }
    private static string CreateSecret() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));

    private async Task<bool> IsEmailInUseAsync(string email)
    {
        var normalizedEmail = userManager.NormalizeEmail(email);
        return await dbContext.Users.IgnoreQueryFilters()
            .AnyAsync(user => user.NormalizedEmail == normalizedEmail);
    }
}
