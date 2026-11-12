using CourseLibrary.CustomerManagement.Data;
using CourseLibrary.CustomerManagement.Domain.Entities;
using CourseLibrary.CustomerManagement.Infrastructure;
using CourseLibrary.CustomerManagement.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Net.Sockets;
using MailKit.Net.Smtp;

namespace CourseLibrary.CustomerManagement.Controllers;

public sealed class InvitationsController(
    CustomerManagementDbContext dbContext,
    IEmailSender emailSender,
    IConfiguration configuration) : Controller
{
    [Authorize]
    [HttpGet("organizations/{organizationId:guid}/invitations/create")]
    public async Task<IActionResult> Create(Guid organizationId, CancellationToken cancellationToken)
    {
        var access = await GetInvitationAccessAsync(organizationId, cancellationToken);
        if (access is null)
            return NotFound();
        if (!access.CanInvite)
            return Forbid();

        return View(new CreateInvitationViewModel
        {
            OrganizationId = organizationId,
            OrganizationName = access.OrganizationName
        });
    }

    [Authorize]
    [HttpPost("organizations/{organizationId:guid}/invitations/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        Guid organizationId,
        CreateInvitationViewModel model,
        CancellationToken cancellationToken)
    {
        var access = await GetInvitationAccessAsync(organizationId, cancellationToken);
        if (access is null)
            return NotFound();
        if (!access.CanInvite)
            return Forbid();

        model.OrganizationId = organizationId;
        model.OrganizationName = access.OrganizationName;
        if (!ModelState.IsValid)
            return View(model);
        if (model.Role is not ("Administrator" or "Member"))
        {
            ModelState.AddModelError(nameof(model.Role), "Select an available organization role.");
            return View(model);
        }

        var normalizedEmail = NormalizeEmail(model.Email);
        if (await dbContext.OrganizationMemberships.AnyAsync(
                item => item.OrganizationId == organizationId &&
                    item.NormalizedEmail == normalizedEmail,
                cancellationToken))
        {
            ModelState.AddModelError(nameof(model.Email), "This person is already a member of this organization.");
            return View(model);
        }

        var now = DateTimeOffset.UtcNow;
        var activeInvitation = await dbContext.OrganizationInvitations.AnyAsync(
            item => item.OrganizationId == organizationId &&
                item.NormalizedEmail == normalizedEmail &&
                item.AcceptedAt == null &&
                item.RevokedAt == null &&
                item.ExpiresAt > now,
            cancellationToken);
        if (activeInvitation)
        {
            ModelState.AddModelError(nameof(model.Email), "An active invitation already exists for this email.");
            return View(model);
        }

        var userId = GetUserId();
        var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        var invitation = new OrganizationInvitation
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Email = model.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            Role = model.Role,
            InvitedByUserId = userId,
            TokenHash = HashToken(token),
            CreatedAt = now,
            ExpiresAt = now.AddHours(Math.Clamp(
                configuration.GetValue("Security:InvitationLifetimeHours", 72),
                1,
                720))
        };

        dbContext.OrganizationInvitations.Add(invitation);
        await dbContext.SaveChangesAsync(cancellationToken);

        var relativeLink = Url.Action(
            nameof(Accept),
            "Invitations",
            new { token })
            ?? throw new InvalidOperationException("Could not create an organization invitation link.");
        if (!Uri.TryCreate(configuration["PublicBaseUrl"], UriKind.Absolute, out var publicBaseUri) ||
            publicBaseUri.Scheme != Uri.UriSchemeHttps)
            throw new InvalidOperationException("PublicBaseUrl must be configured as an absolute HTTPS URL.");
        var link = new Uri(publicBaseUri, relativeLink).AbsoluteUri;
        try
        {
            await emailSender.SendAsync(
                invitation.Email,
                $"Join {access.OrganizationName}",
                $"You have been invited to join {access.OrganizationName} as a {invitation.Role}.\n\nAccept the invitation: {link}\n\nThis link expires at {invitation.ExpiresAt:O}.",
                cancellationToken);
        }
        catch (Exception exception) when (
            exception is SmtpCommandException or
                SmtpProtocolException or
                SocketException or
                IOException or
                InvalidOperationException)
        {
            invitation.RevokedAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            ModelState.AddModelError(
                string.Empty,
                "The invitation was not sent. Check the email configuration and try again.");
            return View(model);
        }

        TempData["Success"] = $"Invitation sent to {invitation.Email}.";
        return RedirectToAction(nameof(OrganizationsController.Details), "Organizations", new { id = organizationId });
    }

    [AllowAnonymous]
    [HttpGet("invitations/accept/{token}")]
    public async Task<IActionResult> Accept(string token, CancellationToken cancellationToken)
    {
        var invitation = await FindInvitationAsync(token, cancellationToken);
        if (invitation is null)
            return View("Accept", new AcceptInvitationViewModel { Token = token, Error = "This invitation is invalid or has expired." });

        if (User.Identity?.IsAuthenticated != true)
        {
            var returnUrl = Url.Action(nameof(Accept), "Invitations", new { token })
                ?? throw new InvalidOperationException("Could not create the invitation return URL.");
            return Challenge(
                new AuthenticationProperties { RedirectUri = returnUrl },
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        var model = new AcceptInvitationViewModel
        {
            Token = token,
            OrganizationName = invitation.Organization!.Name,
            InvitationEmail = invitation.Email,
            SignedInEmail = User.FindFirstValue("email")
        };
        if (!EmailMatchesInvitation(model.SignedInEmail, invitation.NormalizedEmail))
        {
            model.Error = $"Sign in with {invitation.Email} to accept this invitation.";
            return View("Accept", model);
        }

        return View("Accept", model);
    }

    [Authorize]
    [HttpPost("invitations/accept")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(
        AcceptInvitationViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var invitation = await FindInvitationAsync(model.Token, cancellationToken);
        if (invitation is null)
        {
            model.Error = "This invitation is invalid or has expired.";
            return View(model);
        }

        var email = User.FindFirstValue("email");
        if (!EmailMatchesInvitation(email, invitation.NormalizedEmail))
        {
            model.OrganizationName = invitation.Organization!.Name;
            model.InvitationEmail = invitation.Email;
            model.SignedInEmail = email;
            model.Error = $"Sign in with {invitation.Email} to accept this invitation.";
            return View(model);
        }

        var userId = GetUserId();
        var membership = await dbContext.OrganizationMemberships.SingleOrDefaultAsync(
            item => item.OrganizationId == invitation.OrganizationId && item.UserId == userId,
            cancellationToken);
        if (membership is null)
        {
            dbContext.OrganizationMemberships.Add(new OrganizationMembership
            {
                OrganizationId = invitation.OrganizationId,
                UserId = userId,
                Email = email!,
                NormalizedEmail = invitation.NormalizedEmail,
                Role = invitation.Role,
                JoinedAt = DateTimeOffset.UtcNow
            });
        }

        invitation.AcceptedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return RedirectToAction(
            nameof(OrganizationsController.Details),
            "Organizations",
            new { id = invitation.OrganizationId });
    }

    private async Task<OrganizationInvitation?> FindInvitationAsync(
        string token,
        CancellationToken cancellationToken) =>
        await dbContext.OrganizationInvitations
            .Include(item => item.Organization)
            .SingleOrDefaultAsync(
                item => item.TokenHash == HashToken(token) &&
                    item.AcceptedAt == null &&
                    item.RevokedAt == null &&
                    item.ExpiresAt > DateTimeOffset.UtcNow,
                cancellationToken);

    private async Task<InvitationAccess?> GetInvitationAccessAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        return await dbContext.OrganizationMemberships
            .AsNoTracking()
            .Where(item => item.OrganizationId == organizationId && item.UserId == userId)
            .Select(item => new InvitationAccess(
                item.Organization!.Name,
                item.Role == "Owner" || item.Role == "Administrator"))
            .SingleOrDefaultAsync(cancellationToken);
    }

    private string GetUserId() =>
        User.FindFirstValue("sub")
        ?? throw new InvalidOperationException("The authenticated identity has no subject claim.");

    private static bool EmailMatchesInvitation(string? email, string normalizedInvitationEmail) =>
        email is not null &&
        string.Equals(NormalizeEmail(email), normalizedInvitationEmail, StringComparison.Ordinal);

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();
    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private sealed record InvitationAccess(string OrganizationName, bool CanInvite);
}
