using Microsoft.AspNetCore.Authorization;

namespace CourseLibrary.Gateway.Configuration.Authorization;

internal static class GatewayAuthorizationConstants
{
    public const string UserPolicy = "GatewayUser";
    public const string M2MPolicy = "GatewayM2M";
    public const string UserOrM2MPolicy = "GatewayUserOrM2M";
}

internal static class GatewayAuthorizationExtensions
{
    public static WebApplicationBuilder AddGatewayAuthorization(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(
                GatewayAuthorizationConstants.UserPolicy,
                policy =>
                {
                    policy.RequireAuthenticatedUser();

                    policy.AddRequirements(
                        new UserTokenRequirement());
                });

            options.AddPolicy(
                GatewayAuthorizationConstants.M2MPolicy,
                policy =>
                {
                    policy.RequireAuthenticatedUser();

                    policy.AddRequirements(
                        new M2MClientRequirement());
                });

            options.AddPolicy(
                GatewayAuthorizationConstants.UserOrM2MPolicy,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                });
        });

        //builder.Services.AddScoped<
        //    IAuthorizationHandler,
        //    UserTokenAuthorizationHandler>();

        //builder.Services.AddScoped<
        //    IAuthorizationHandler,
        //    M2MClientAuthorizationHandler>();

        builder.Services.AddSingleton<IAuthorizationHandler, ApiAccessAuthorizationHandler>();

        return builder;
    }
}
