using CourseLibrary.Idp.Domain.Entities;
using CourseLibrary.Idp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.Idp.Controllers;

public sealed class InvitationController(UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet("invite/accept")]
    [AllowAnonymous]
    public IActionResult AcceptInvitation(string email, string token) => View(new AcceptInvitationViewModel { Email = email, Token = token });

    [HttpPost("invite/accept")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptInvitation(AcceptInvitationViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user is null) { ModelState.AddModelError(string.Empty, "The invitation is invalid or has expired."); return View(model); }
        var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
        if (!result.Succeeded) { ModelState.AddModelError(string.Empty, "The invitation is invalid or has expired."); return View(model); }
        user.EmailConfirmed = true;
        await userManager.UpdateAsync(user);
        TempData["Success"] = "Your account is ready. Sign in to continue.";
        return RedirectToAction("Login", "Account");
    }
}
