using CourseLibrary.Idp.Domain.Entities;
using CourseLibrary.Idp.Models;
using CourseLibrary.Idp.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace CourseLibrary.Idp.Controllers;

public sealed class AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IAuthenticationSchemeProvider schemes, IEmailSender emailSender, ILogger<AccountController> logger) : Controller
{
    [HttpGet("account/register")]
    [AllowAnonymous]
    public IActionResult Register(string? returnUrl = null) => View(new RegisterViewModel { ReturnUrl = returnUrl });

    [HttpGet("admin/login")]
    [AllowAnonymous]
    public IActionResult AdminLogin(string? returnUrl = null) => View("Login", new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost("admin/login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdminLogin(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View("Login", model);
        var user = await userManager.FindByNameAsync(model.UserName);
        if (user is null || !await userManager.IsInRoleAsync(user, "Administrator")) { ModelState.AddModelError(string.Empty, "Administrator credentials are invalid."); return View("Login", model); }
        var result = await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (result.RequiresTwoFactor) return RedirectToAction(nameof(LoginWithAuthenticator), new { model.ReturnUrl, model.RememberMe });
        if (!result.Succeeded) { ModelState.AddModelError(string.Empty, "Administrator credentials are invalid."); return View("Login", model); }
        if (!user.TwoFactorEnabled) { await signInManager.SignOutAsync(); TempData["Error"] = "Administrators must enable two-factor authentication."; return RedirectToAction(nameof(EnableAuthenticator)); }
        return LocalRedirectOrHome(model.ReturnUrl);
    }

    [HttpPost("account/register")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName, CreatedAt = DateTimeOffset.UtcNow, LockoutEnabled = true };
        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded) { AddErrors(result); return View(model); }
        await SendConfirmationEmailAsync(user, model.ReturnUrl);
        return View("RegistrationConfirmation");
    }

    [HttpGet("account/confirm-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return View("EmailConfirmation", false);
        var result = await userManager.ConfirmEmailAsync(user, token);
        return View("EmailConfirmation", result.Succeeded);
    }

    [HttpPost("account/resend-confirmation")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendConfirmation(EmailViewModel model)
    {
        if (ModelState.IsValid && await userManager.FindByEmailAsync(model.Email) is { } user && !await userManager.IsEmailConfirmedAsync(user))
            await SendConfirmationEmailAsync(user, model.ReturnUrl);
        return View("RegistrationConfirmation");
    }

    [HttpGet("account/forgot-password")]
    [AllowAnonymous]
    public IActionResult ForgotPassword() => View(new EmailViewModel());

    [HttpPost("account/forgot-password")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(EmailViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user is not null && await userManager.IsEmailConfirmedAsync(user))
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var link = Url.Action(nameof(ResetPassword), "Account", new { email = user.Email, token }, Request.Scheme)!;
            await emailSender.SendAsync(user.Email!, "Reset your password", link);
        }
        return View("ForgotPasswordConfirmation");
    }

    [HttpGet("account/reset-password")]
    [AllowAnonymous]
    public IActionResult ResetPassword(string email, string token) => View(new ResetPasswordViewModel { Email = email, Token = token });

    [HttpPost("account/reset-password")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user is null) return View("ResetPasswordConfirmation");
        var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
        if (!result.Succeeded) { AddErrors(result); return View(model); }
        await userManager.UpdateSecurityStampAsync(user);
        return View("ResetPasswordConfirmation");
    }

    [HttpGet("account/profile")]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await CurrentUserAsync();
        return View(new ProfileViewModel { FullName = user.FullName, Email = user.Email!, UserName = user.UserName });
    }

    [HttpPost("account/profile")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await CurrentUserAsync();
        user.FullName = model.FullName;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded) { AddErrors(result); return View(model); }
        await signInManager.RefreshSignInAsync(user);
        TempData["Success"] = "Profile updated.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet("account/change-password")]
    [Authorize]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost("account/change-password")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await userManager.ChangePasswordAsync(await CurrentUserAsync(), model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded) { AddErrors(result); return View(model); }
        await signInManager.RefreshSignInAsync(await CurrentUserAsync());
        TempData["Success"] = "Password changed.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost("account/change-email")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel model)
    {
        if (!ModelState.IsValid) return RedirectToAction(nameof(Profile));
        var user = await CurrentUserAsync();
        if (!await userManager.CheckPasswordAsync(user, model.Password)) { TempData["Error"] = "The password is invalid."; return RedirectToAction(nameof(Profile)); }
        var token = await userManager.GenerateChangeEmailTokenAsync(user, model.NewEmail);
        var link = Url.Action(nameof(ConfirmEmailChange), "Account", new { userId = user.Id, email = model.NewEmail, token }, Request.Scheme)!;
        await emailSender.SendAsync(model.NewEmail, "Confirm your new email address", link);
        TempData["Success"] = "Check your new email address to confirm the change.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet("account/confirm-email-change")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmailChange(string userId, string email, string token)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return View("EmailConfirmation", false);
        var result = await userManager.ChangeEmailAsync(user, email, token);
        if (result.Succeeded) { user.UserName = email; await userManager.UpdateAsync(user); }
        return View("EmailConfirmation", result.Succeeded);
    }

    [HttpGet("account/external-logins")]
    [Authorize]
    public async Task<IActionResult> ExternalLogins() => View(await GetExternalLoginsAsync());

    [HttpPost("account/external-logins/link")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LinkExternalLogin(string provider) => Challenge(signInManager.ConfigureExternalAuthenticationProperties(provider, Url.Action(nameof(LinkExternalLoginCallback))!));

    [HttpGet("account/external-logins/callback")]
    [Authorize]
    public async Task<IActionResult> LinkExternalLoginCallback()
    {
        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null) { TempData["Error"] = "Could not read the external login."; return RedirectToAction(nameof(ExternalLogins)); }
        var result = await userManager.AddLoginAsync(await CurrentUserAsync(), info);
        if (!result.Succeeded) AddErrors(result);
        return RedirectToAction(nameof(ExternalLogins));
    }

    [HttpPost("account/external-logins/unlink")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnlinkExternalLogin(string provider, string key)
    {
        var user = await CurrentUserAsync();
        if (await userManager.GetLoginsAsync(user) is { Count: <= 1 } && string.IsNullOrEmpty(user.PasswordHash))
            TempData["Error"] = "Keep another sign-in method before unlinking this provider.";
        else await userManager.RemoveLoginAsync(user, provider, key);
        return RedirectToAction(nameof(ExternalLogins));
    }

    [HttpPost("account/2fa/disable")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DisableTwoFactor() { await userManager.SetTwoFactorEnabledAsync(await CurrentUserAsync(), false); return RedirectToAction(nameof(Profile)); }

    [HttpPost("account/2fa/recovery-codes")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegenerateRecoveryCodes() { TempData["RecoveryCodes"] = string.Join(" ", await userManager.GenerateNewTwoFactorRecoveryCodesAsync(await CurrentUserAsync(), 10) ?? []); return RedirectToAction(nameof(Profile)); }

    [HttpPost("account/delete")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount(DeleteAccountViewModel model)
    {
        if (!ModelState.IsValid) return RedirectToAction(nameof(Profile));
        var user = await CurrentUserAsync();
        if (!await userManager.CheckPasswordAsync(user, model.Password)) { TempData["Error"] = "The password is invalid."; return RedirectToAction(nameof(Profile)); }
        user.DeletedAt = DateTimeOffset.UtcNow; user.LockoutEnabled = true; user.LockoutEnd = DateTimeOffset.MaxValue;
        await userManager.UpdateAsync(user); await signInManager.SignOutAsync();
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }
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

    private async Task<ApplicationUser> CurrentUserAsync() => await userManager.GetUserAsync(User)
        ?? throw new InvalidOperationException("User not found.");

    private async Task<List<ExternalLoginItem>> GetExternalLoginsAsync()
    {
        var logins = await userManager.GetLoginsAsync(await CurrentUserAsync());
        return logins.Select(x => new ExternalLoginItem { LoginProvider = x.LoginProvider, ProviderKey = x.ProviderKey, ProviderDisplayName = x.ProviderDisplayName ?? x.LoginProvider }).ToList();
    }

    private async Task SendConfirmationEmailAsync(ApplicationUser user, string? returnUrl)
    {
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var link = Url.Action(nameof(ConfirmEmail), "Account", new { userId = user.Id, token, returnUrl }, Request.Scheme)!;
        await emailSender.SendAsync(user.Email!, "Confirm your email address", link);
        logger.LogInformation("Confirmation email created for user {UserId}.", user.Id);
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
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
