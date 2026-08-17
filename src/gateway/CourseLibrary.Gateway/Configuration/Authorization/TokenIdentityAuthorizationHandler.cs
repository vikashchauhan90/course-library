using CourseLibrary.Gateway.Configuration.Authentication;
using Microsoft.AspNetCore.Authorization;

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
            _ => false
        };

        if (isSatisfied)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
