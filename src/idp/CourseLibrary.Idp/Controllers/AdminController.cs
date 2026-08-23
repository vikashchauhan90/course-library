using CourseLibrary.Idp.Domain.Entities;
using CourseLibrary.Idp.Models.Admin;
using CourseLibrary.Idp.Models;
using CourseLibrary.Idp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace CourseLibrary.Idp.Controllers;

[Authorize(Roles = "Administrator")]
[Route("admin")]
public sealed class AdminController(
    UserManager<ApplicationUser> userManager,
    IOpenIddictApplicationManager applicationManager,
    IOpenIddictScopeManager scopeManager,
    ApplicationDbContext dbContext,
    ILogger<AdminController> logger) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var users = await userManager.Users.OrderBy(x => x.UserName).Take(100).ToListAsync();
        var userItems = new List<AdminUserItem>(users.Count);
        foreach (var user in users)
            userItems.Add(new(user.Id, user.UserName ?? user.Id, user.Email ?? string.Empty,
                user.LockoutEnd > DateTimeOffset.UtcNow, await userManager.IsInRoleAsync(user, "Administrator")));

        var clients = new List<AdminClientItem>();
        await foreach (var client in applicationManager.ListAsync())
            clients.Add(new(await applicationManager.GetClientIdAsync(client) ?? string.Empty,
                await applicationManager.GetDisplayNameAsync(client)));

        var scopes = new List<AdminScopeItem>();
        await foreach (var scope in scopeManager.ListAsync())
        {
            var resources = await scopeManager.GetResourcesAsync(scope);
            scopes.Add(new(await scopeManager.GetNameAsync(scope) ?? string.Empty,
                await scopeManager.GetDisplayNameAsync(scope), string.Join(", ", resources)));
        }

        return View(new AdminIndexViewModel { Users = userItems, Clients = clients, Scopes = scopes });
    }

    [HttpGet("users/create")]
    public IActionResult CreateUser() => View(new CreateUserViewModel());

    [HttpGet("users/invite")]
    public IActionResult InviteUser() => View(new InviteUserViewModel());

    [HttpPost("users/invite")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InviteUser(InviteUserViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (await userManager.FindByEmailAsync(model.Email) is not null)
        { ModelState.AddModelError(nameof(model.Email), "A user with this email already exists."); return View(model); }

        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName, CreatedAt = DateTimeOffset.UtcNow, LockoutEnabled = true };
        var result = await userManager.CreateAsync(user);
        if (!result.Succeeded) { AddErrors(result); return View(model); }
        if (model.IsAdministrator)
        {
            result = await userManager.AddToRoleAsync(user, "Administrator");
            if (!result.Succeeded) { AddErrors(result); return View(model); }
        }
        var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        var activeInvitations = await dbContext.UserInvitations
            .Where(x => x.UserId == user.Id && x.AcceptedAt == null && x.RevokedAt == null)
            .ToListAsync();
        foreach (var invitation in activeInvitations) invitation.RevokedAt = DateTimeOffset.UtcNow;
        dbContext.UserInvitations.Add(new()
        {
            Id = Guid.NewGuid().ToString(),
            UserId = user.Id,
            TokenHash = HashToken(token),
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(Math.Clamp(HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetValue("Security:InvitationLifetimeHours", 72), 1, 720))
        });
        await dbContext.SaveChangesAsync();
        var link = Url.Action("AcceptInvitation", "Invitation", new { email = user.Email, token }, Request.Scheme)
            ?? throw new InvalidOperationException("Could not create invitation link.");
        logger.LogInformation("Administrator {AdministratorId} invited user {UserId}.", userManager.GetUserId(User), user.Id);
        TempData["InvitationLink"] = link;
        TempData["Success"] = "Invitation created. It is an opaque, single-use token and expires at the configured time.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("users/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, FullName = model.FullName, EmailConfirmed = true, CreatedAt = DateTimeOffset.UtcNow };
        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded) { AddErrors(result); return View(model); }
        if (model.IsAdministrator)
        {
            result = await userManager.AddToRoleAsync(user, "Administrator");
            if (!result.Succeeded) { AddErrors(result); return View(model); }
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

    [HttpGet("clients/create")]
    public IActionResult CreateClient() => View(new CreateClientViewModel());

    [HttpPost("clients/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateClient(CreateClientViewModel model)
    {
        Uri? redirectUri = null;
        if (!model.AllowClientCredentials && !model.AllowAuthorizationCode)
            ModelState.AddModelError(string.Empty, "Select at least one grant type.");
        if (model.AllowAuthorizationCode && !Uri.TryCreate(model.RedirectUri, UriKind.Absolute, out redirectUri))
            ModelState.AddModelError(nameof(model.RedirectUri), "An absolute redirect URI is required for authorization code flow.");
        if (!ModelState.IsValid) return View(model);
        if (await applicationManager.FindByClientIdAsync(model.ClientId) is not null)
        { ModelState.AddModelError(nameof(model.ClientId), "That client ID already exists."); return View(model); }

        var secret = CreateSecret();
        var descriptor = new OpenIddictApplicationDescriptor { ClientId = model.ClientId, ClientSecret = secret, DisplayName = model.DisplayName };
        descriptor.Permissions.Add(Permissions.Endpoints.Token);
        descriptor.Permissions.Add(Permissions.Prefixes.Scope + "course-library-api");
        if (model.AllowClientCredentials) descriptor.Permissions.Add(Permissions.GrantTypes.ClientCredentials);
        if (model.AllowAuthorizationCode)
        {
            descriptor.RedirectUris.Add(redirectUri!);
            descriptor.Permissions.Add(Permissions.Endpoints.Authorization);
            descriptor.Permissions.Add(Permissions.GrantTypes.AuthorizationCode);
            descriptor.Permissions.Add(Permissions.ResponseTypes.Code);
            descriptor.Requirements.Add(Requirements.Features.ProofKeyForCodeExchange);
        }
        await applicationManager.CreateAsync(descriptor);
        logger.LogInformation("Administrator {AdministratorId} created OAuth client {ClientId}.", userManager.GetUserId(User), model.ClientId);
        TempData["ClientSecret"] = secret;
        TempData["Success"] = "Client created. Copy its secret now; it will not be displayed again.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("clients/{clientId}/rotate-secret")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RotateClientSecret(string clientId)
    {
        var client = await applicationManager.FindByClientIdAsync(clientId);
        if (client is null) return NotFound();
        var descriptor = new OpenIddictApplicationDescriptor();
        await applicationManager.PopulateAsync(descriptor, client);
        var secret = CreateSecret();
        descriptor.ClientSecret = secret;
        await applicationManager.UpdateAsync(client, descriptor);
        logger.LogInformation("Administrator {AdministratorId} rotated the secret for OAuth client {ClientId}.", userManager.GetUserId(User), clientId);
        TempData["ClientSecret"] = secret;
        TempData["Success"] = "Client secret rotated. Copy the new value now; it will not be displayed again.";
        return RedirectToAction(nameof(Index));
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
}
