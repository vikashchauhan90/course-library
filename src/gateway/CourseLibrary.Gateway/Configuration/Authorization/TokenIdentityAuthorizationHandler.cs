using CourseLibrary.Gateway.Configuration.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CourseLibrary.Gateway.Configuration.Authorization;

internal sealed class TokenIdentityAuthorizationHandler(
    ITokenIdentityService tokenIdentityService)
    : AuthorizationHandler<IAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        IAuthorizationRequirement requirement)
    {
        var identityType = tokenIdentityService.GetIdentityType(context.User);

        var isSatisfied = requirement switch
        {
            UserTokenRequirement => identityType == TokenIdentityType.User,
            M2MClientRequirement => identityType == TokenIdentityType.M2M,
            UserOrM2MRequirement => identityType is TokenIdentityType.User or TokenIdentityType.M2M,
            AdministratorRequirement => IsAdministrator(context.User),
            GatewayPermissionRequirement permission =>
                identityType == TokenIdentityType.M2M
                || HasPermission(context.User, permission),
            _ => false
        };

        if (isSatisfied)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    private static bool HasPermission(
        ClaimsPrincipal principal,
        GatewayPermissionRequirement requirement)
    {
        if (IsAdministrator(principal))
            return true;

        var requiredPermission = GatewayPermission.For(
            requirement.Resource,
            requirement.Action);
        var allResourcePermission = GatewayPermission.For(
            requirement.Resource,
            GatewayPermission.All);

        return principal.Claims.Any(claim =>
            claim.Type.Equals(
                GatewayPermission.ClaimType,
                StringComparison.OrdinalIgnoreCase)
            && (claim.Value.Equals(
                    requiredPermission,
                    StringComparison.OrdinalIgnoreCase)
                || claim.Value.Equals(
                    allResourcePermission,
                    StringComparison.OrdinalIgnoreCase)
                || claim.Value.Equals(
                    $"*.{GatewayPermission.All}",
                    StringComparison.OrdinalIgnoreCase)));
    }

    private static bool IsAdministrator(ClaimsPrincipal principal)
    {
        return principal.IsInRole(GatewayPermission.AdministratorRole)
            || principal.Claims.Any(claim =>
                claim.Type.Equals(
                    GatewayPermission.ClaimType,
                    StringComparison.OrdinalIgnoreCase)
                && claim.Value.Equals(
                    $"*.{GatewayPermission.All}",
                    StringComparison.OrdinalIgnoreCase));
    }
}
