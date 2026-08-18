using CourseLibrary.Idp.Domain.Entities;
using CourseLibrary.Idp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.Idp.Controllers;

public sealed class AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager) : Controller
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
}
