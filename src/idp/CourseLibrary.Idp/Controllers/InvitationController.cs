using CourseLibrary.Idp.Domain.Entities;
using CourseLibrary.Idp.Models;
using CourseLibrary.Idp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseLibrary.Idp.Controllers;

public sealed class InvitationController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext) : Controller
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
        var invitation = await dbContext.UserInvitations.SingleOrDefaultAsync(x => x.TokenHash == HashToken(model.Token));
        if (invitation is null || invitation.AcceptedAt is not null || invitation.RevokedAt is not null || invitation.ExpiresAt <= DateTimeOffset.UtcNow)
        { ModelState.AddModelError(string.Empty, "The invitation is invalid or has expired."); return View(model); }
        var user = await userManager.FindByIdAsync(invitation.UserId);
        if (user is null || !string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
        { ModelState.AddModelError(string.Empty, "The invitation is invalid or has expired."); return View(model); }
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, model.Password);
        if (!result.Succeeded) { ModelState.AddModelError(string.Empty, "The invitation could not be accepted."); return View(model); }
        user.EmailConfirmed = true;
        invitation.AcceptedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Your account is ready. Sign in to continue.";
        return RedirectToAction("Login", "Account");
    }

    private static string HashToken(string token) => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));
}
