using CourseLibrary.Idp.Domain.Entities;
using CourseLibrary.Idp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using System.Security.Claims;
using System.Collections.Immutable;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace CourseLibrary.Idp.Controllers;

public sealed class AuthorizationController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IOpenIddictApplicationManager applicationManager) : Controller
{
    [HttpGet("connect/authorize")]
    [HttpPost("connect/authorize")]
    [Authorize]
    public async Task<IActionResult> Authorize()
    {
        var request = Microsoft.AspNetCore.OpenIddictServerAspNetCoreHelpers.GetOpenIddictServerRequest(HttpContext)
            ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        if (Request.Method == HttpMethods.Get)
        {
            var client = request.ClientId is null ? null : await applicationManager.FindByClientIdAsync(request.ClientId);
            return View("Consent", new ConsentViewModel
            {
                ClientName = client is null ? request.ClientId ?? "Application" : await applicationManager.GetDisplayNameAsync(client) ?? request.ClientId!,
                Scopes = request.GetScopes().ToList()
            });
        }
        if (!string.Equals(Request.Form["decision"], "approve", StringComparison.OrdinalIgnoreCase))
            return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        var identity = await CreateUserIdentityAsync(user, request.GetScopes());
        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("connect/token")]
    [AllowAnonymous]
    public async Task<IActionResult> Exchange()
    {
        var request = Microsoft.AspNetCore.OpenIddictServerAspNetCoreHelpers.GetOpenIddictServerRequest(HttpContext)
            ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsClientCredentialsGrantType())
        {
            var client = await applicationManager.FindByClientIdAsync(request.ClientId!);
            if (client is null) return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            identity.SetClaim(Claims.Subject, $"{request.ClientId}@clients");
            identity.SetClaim(Claims.AuthorizedParty, request.ClientId);
            identity.SetScopes(request.GetScopes());
            identity.SetResources("course-library-api");
            SetDestinations(identity);
            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (result.Principal is null)
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            return SignIn(result.Principal!, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpGet("connect/userinfo")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> UserInfo()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        var roles = await userManager.GetRolesAsync(user);
        return Ok(new { sub = user.Id, name = user.UserName, email = user.Email, role = roles });
    }

    [HttpGet("connect/logout")]
    [HttpPost("connect/logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return SignOut(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user, IEnumerable<string> requestedScopes)
    {
        var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        identity.SetClaim(Claims.Subject, user.Id);
        identity.SetClaim(Claims.Name, user.UserName);
        identity.SetClaim(Claims.Email, user.Email);
        identity.SetClaims(Claims.Role, (await userManager.GetRolesAsync(user)).ToImmutableArray());
        identity.SetScopes(requestedScopes);
        identity.SetResources("course-library-api");
        SetDestinations(identity);
        return identity;
    }

    private static void SetDestinations(ClaimsIdentity identity)
    {
        foreach (var claim in identity.Claims)
        {
            claim.SetDestinations(claim.Type switch
            {
                Claims.Name when identity.HasScope(Scopes.Profile) => [Destinations.AccessToken, Destinations.IdentityToken],
                Claims.Email when identity.HasScope(Scopes.Email) => [Destinations.AccessToken, Destinations.IdentityToken],
                Claims.Role when identity.HasScope(Scopes.Roles) => [Destinations.AccessToken, Destinations.IdentityToken],
                Claims.Subject or Claims.AuthorizedParty => [Destinations.AccessToken, Destinations.IdentityToken],
                _ => [Destinations.AccessToken]
            });
        }
    }
}
