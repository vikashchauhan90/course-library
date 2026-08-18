using CourseLibrary.Idp.Domain.Entities;
using CourseLibrary.Idp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace CourseLibrary.Idp.Controllers;

public sealed class AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IAuthenticationSchemeProvider schemes) : Controller
{
    [HttpGet("account/login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost("account/login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded) return LocalRedirectOrHome(model.ReturnUrl);
        if (result.RequiresTwoFactor) return RedirectToAction(nameof(LoginWithAuthenticator), new { model.ReturnUrl, model.RememberMe });
        ModelState.AddModelError(string.Empty, result.IsLockedOut
            ? "This account is temporarily locked."
            : "The username or password is invalid.");
        return View(model);
    }

    [HttpPost("account/external-login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExternalLogin(string provider, string? returnUrl = null)
    {
        var scheme = await schemes.GetSchemeAsync(provider);
        if (scheme is null || !await IsExternalSchemeAsync(provider)) return NotFound();
        return Challenge(signInManager.ConfigureExternalAuthenticationProperties(provider, Url.Action(nameof(ExternalLoginCallback), new { returnUrl })!));
    }

    [HttpGet("account/external-login-callback")]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (!string.IsNullOrWhiteSpace(remoteError)) { TempData["Error"] = "External sign-in was cancelled or failed."; return RedirectToAction(nameof(Login), new { returnUrl }); }
        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null) { TempData["Error"] = "Could not read the external sign-in result."; return RedirectToAction(nameof(Login), new { returnUrl }); }
        var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: false);
        if (result.Succeeded) return LocalRedirectOrHome(returnUrl);
        if (result.RequiresTwoFactor) return RedirectToAction(nameof(LoginWithAuthenticator), new { returnUrl });
        var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? info.Principal.FindFirstValue("email");
        if (string.IsNullOrWhiteSpace(email)) { TempData["Error"] = "This provider did not provide a verified email address."; return RedirectToAction(nameof(Login), new { returnUrl }); }
        if (await userManager.FindByEmailAsync(email) is not null) { TempData["Error"] = "An account with this email already exists. Sign in with its existing method first."; return RedirectToAction(nameof(Login), new { returnUrl }); }
        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, FullName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email, CreatedAt = DateTimeOffset.UtcNow, LockoutEnabled = true };
        var created = await userManager.CreateAsync(user);
        if (!created.Succeeded) { TempData["Error"] = "Could not create your account."; return RedirectToAction(nameof(Login), new { returnUrl }); }
        var linked = await userManager.AddLoginAsync(user, info);
        if (!linked.Succeeded) { await userManager.DeleteAsync(user); TempData["Error"] = "Could not link the external account."; return RedirectToAction(nameof(Login), new { returnUrl }); }
        await signInManager.SignInAsync(user, isPersistent: false);
        return LocalRedirectOrHome(returnUrl);
    }

    [HttpPost("account/logout")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    [HttpGet("account/login-2fa")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginWithAuthenticator(string? returnUrl = null, bool rememberMe = false)
    {
        if (await signInManager.GetTwoFactorAuthenticationUserAsync() is null) return RedirectToAction(nameof(Login));
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["RememberMe"] = rememberMe;
        return View(new VerifyAuthenticatorViewModel());
    }

    [HttpPost("account/login-2fa")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginWithAuthenticator(VerifyAuthenticatorViewModel model, string? returnUrl = null, bool rememberMe = false)
    {
        if (!ModelState.IsValid) { ViewData["ReturnUrl"] = returnUrl; ViewData["RememberMe"] = rememberMe; return View(model); }
        var result = await signInManager.TwoFactorAuthenticatorSignInAsync(model.Code.Replace(" ", string.Empty).Replace("-", string.Empty), rememberMe, model.RememberMachine);
        if (result.Succeeded) return LocalRedirectOrHome(returnUrl);
        ModelState.AddModelError(string.Empty, result.IsLockedOut ? "This account is temporarily locked." : "The authentication code is invalid.");
        ViewData["ReturnUrl"] = returnUrl; ViewData["RememberMe"] = rememberMe;
        return View(model);
    }

    [HttpGet("account/login-recovery")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginWithRecoveryCode(string? returnUrl = null)
    {
        if (await signInManager.GetTwoFactorAuthenticationUserAsync() is null) return RedirectToAction(nameof(Login));
        ViewData["ReturnUrl"] = returnUrl;
        return View(new RecoveryCodeViewModel());
    }

    [HttpPost("account/login-recovery")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginWithRecoveryCode(RecoveryCodeViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) { ViewData["ReturnUrl"] = returnUrl; return View(model); }
        var result = await signInManager.TwoFactorRecoveryCodeSignInAsync(model.Code.Replace(" ", string.Empty));
        if (result.Succeeded) return LocalRedirectOrHome(returnUrl);
        ModelState.AddModelError(string.Empty, "The recovery code is invalid.");
        ViewData["ReturnUrl"] = returnUrl;
        return View(model);
    }

    [HttpGet("account/authenticator")]
    [Authorize]
    public async Task<IActionResult> EnableAuthenticator()
    {
        var user = await userManager.GetUserAsync(User) ?? throw new InvalidOperationException("User not found.");
        var key = await userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key)) { await userManager.ResetAuthenticatorKeyAsync(user); key = await userManager.GetAuthenticatorKeyAsync(user); }
        return View(new EnableAuthenticatorViewModel { SharedKey = FormatKey(key!), AuthenticatorUri = CreateUri(user.Email ?? user.UserName ?? user.Id, key!) });
    }

    [HttpPost("account/authenticator")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnableAuthenticator(EnableAuthenticatorViewModel model)
    {
        var user = await userManager.GetUserAsync(User) ?? throw new InvalidOperationException("User not found.");
        if (!ModelState.IsValid) { var key = await userManager.GetAuthenticatorKeyAsync(user) ?? string.Empty; model.SharedKey = FormatKey(key); model.AuthenticatorUri = CreateUri(user.Email ?? user.UserName ?? user.Id, key); return View(model); }
        var valid = await userManager.VerifyTwoFactorTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, model.Code.Replace(" ", string.Empty).Replace("-", string.Empty));
        if (!valid) { ModelState.AddModelError(nameof(model.Code), "The authentication code is invalid."); var key = await userManager.GetAuthenticatorKeyAsync(user) ?? string.Empty; model.SharedKey = FormatKey(key); model.AuthenticatorUri = CreateUri(user.Email ?? user.UserName ?? user.Id, key); return View(model); }
        await userManager.SetTwoFactorEnabledAsync(user, true);
        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        TempData["RecoveryCodes"] = string.Join(" ", recoveryCodes ?? []);
        return RedirectToAction(nameof(EnableAuthenticator));
    }

    private IActionResult LocalRedirectOrHome(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction(nameof(HomeController.Index), "Home")!;

    private static string FormatKey(string key) => string.Join(' ', Enumerable.Range(0, (key.Length + 3) / 4).Select(i => key.Substring(i * 4, Math.Min(4, key.Length - (i * 4))))).ToLowerInvariant();
    private static string CreateUri(string account, string key) => $"otpauth://totp/CourseLibrary:{Uri.EscapeDataString(account)}?secret={key}&issuer=CourseLibrary&digits=6";

    private async Task<bool> IsExternalSchemeAsync(string provider) =>
        (await schemes.GetAllSchemesAsync()).Any(x => x.Name == provider && x.DisplayName is not null);
}
