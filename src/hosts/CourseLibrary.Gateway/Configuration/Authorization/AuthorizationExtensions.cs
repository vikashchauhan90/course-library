using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using CourseLibrary.Gateway.Configuration.Authentication;

namespace CourseLibrary.Gateway.Configuration.Authorization;

internal static class GatewayAuthorizationConstants
{
    public const string PolicyName = "ApiAccess";
}

internal static class GatewayAuthorizationExtensions
{
    public static WebApplicationBuilder AddGatewayAuthorization(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IAuthorizationHandler, ApiAccessAuthorizationHandler>();

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(GatewayAuthorizationConstants.PolicyName, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddAuthenticationSchemes(
                    GatewayAuthenticationConstants.JwtScheme,
                    GatewayAuthenticationConstants.M2MScheme);
                policy.Requirements.Add(new ApiAccessRequirement());
            });
        });

        return builder;
    }
}
