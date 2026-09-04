using CourseLibrary.Idp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace CourseLibrary.Idp;

public sealed class AdminMfaRequirement : IAuthorizationRequirement;

public sealed class AdminMfaHandler(UserManager<ApplicationUser> userManager) : AuthorizationHandler<AdminMfaRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminMfaRequirement requirement)
    {
        if (!context.User.IsInRole("Administrator")) return;
        var user = await userManager.GetUserAsync(context.User);
        if (user?.TwoFactorEnabled == true) context.Succeed(requirement);
    }
}