using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CourseLibrary.Gateway.Configuration.Authorization;

internal sealed class ApiAccessAuthorizationHandler : AuthorizationHandler<ApiAccessRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ApiAccessRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated == true &&
            (context.User.HasClaim(claim => claim.Type == ClaimTypes.NameIdentifier) ||
             context.User.HasClaim(claim => claim.Type == "sub") ||
             context.User.HasClaim(claim => claim.Type == "scope") ||
             context.User.HasClaim(claim => claim.Type == "roles") ||
             context.User.HasClaim(claim => claim.Type == "client_id")))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
