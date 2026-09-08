using Microsoft.AspNetCore.Authorization;

namespace CourseLibrary.Gateway.Configuration.Authorization;

internal static class GatewayAuthorizationConstants
{
    public const string UserPolicy = "GatewayUser";
    public const string M2MPolicy = "GatewayM2M";
    public const string UserOrM2MPolicy = "GatewayUserOrM2M";
    public const string AdministratorPolicy = "GatewayAdministrator";

    public const string CourseReadPolicy = "GatewayCourseRead";
    public const string CourseWritePolicy = "GatewayCourseWrite";
    public const string CourseUpdatePolicy = "GatewayCourseUpdate";
    public const string CourseDeletePolicy = "GatewayCourseDelete";
    public const string CourseAllPolicy = "GatewayCourseAll";

    public const string CommentReadPolicy = "GatewayCommentRead";
    public const string CommentWritePolicy = "GatewayCommentWrite";
    public const string CommentUpdatePolicy = "GatewayCommentUpdate";
    public const string CommentDeletePolicy = "GatewayCommentDelete";
    public const string CommentAllPolicy = "GatewayCommentAll";

    public const string DiscussionReadPolicy = "GatewayDiscussionRead";
    public const string DiscussionWritePolicy = "GatewayDiscussionWrite";
    public const string DiscussionUpdatePolicy = "GatewayDiscussionUpdate";
    public const string DiscussionDeletePolicy = "GatewayDiscussionDelete";
    public const string DiscussionAllPolicy = "GatewayDiscussionAll";
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

                    policy.AddRequirements(
                        new UserOrM2MRequirement());
                });

            options.AddPolicy(
                GatewayAuthorizationConstants.AdministratorPolicy,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.AddRequirements(new AdministratorRequirement());
                });

            AddPermissionPolicy(
                options,
                GatewayAuthorizationConstants.CourseReadPolicy,
                GatewayPermission.Course,
                GatewayPermission.Read);
            AddPermissionPolicy(
                options,
                GatewayAuthorizationConstants.CourseWritePolicy,
                GatewayPermission.Course,
                GatewayPermission.Write);
            AddPermissionPolicy(
                options,
                GatewayAuthorizationConstants.CourseUpdatePolicy,
                GatewayPermission.Course,
                GatewayPermission.Update);
            AddPermissionPolicy(
                options,
                GatewayAuthorizationConstants.CourseDeletePolicy,
                GatewayPermission.Course,
                GatewayPermission.Delete);
            AddPermissionPolicy(
                options,
                GatewayAuthorizationConstants.CourseAllPolicy,
                GatewayPermission.Course,
                GatewayPermission.All);

            AddPermissionPolicy(
                options,
                GatewayAuthorizationConstants.CommentReadPolicy,
                GatewayPermission.Comment,
                GatewayPermission.Read);
            AddPermissionPolicy(
                options,
                GatewayAuthorizationConstants.CommentWritePolicy,
                GatewayPermission.Comment,
                GatewayPermission.Write);
            AddPermissionPolicy(
                options,
                GatewayAuthorizationConstants.CommentUpdatePolicy,
                GatewayPermission.Comment,
                GatewayPermission.Update);
            AddPermissionPolicy(
                options,
                GatewayAuthorizationConstants.CommentDeletePolicy,
                GatewayPermission.Comment,
                GatewayPermission.Delete);
            AddPermissionPolicy(
                options,
                GatewayAuthorizationConstants.CommentAllPolicy,
                GatewayPermission.Comment,
                GatewayPermission.All);

            AddPermissionPolicy(
                options,
                GatewayAuthorizationConstants.DiscussionReadPolicy,
                GatewayPermission.Discussion,
                GatewayPermission.Read);
            AddPermissionPolicy(
                options,
                GatewayAuthorizationConstants.DiscussionWritePolicy,
                GatewayPermission.Discussion,
                GatewayPermission.Write);
            AddPermissionPolicy(
                options,
                GatewayAuthorizationConstants.DiscussionUpdatePolicy,
                GatewayPermission.Discussion,
                GatewayPermission.Update);
            AddPermissionPolicy(
                options,
                GatewayAuthorizationConstants.DiscussionDeletePolicy,
                GatewayPermission.Discussion,
                GatewayPermission.Delete);
            AddPermissionPolicy(
                options,
                GatewayAuthorizationConstants.DiscussionAllPolicy,
                GatewayPermission.Discussion,
                GatewayPermission.All);
        });

        builder.Services.AddSingleton<IAuthorizationHandler,
            TokenIdentityAuthorizationHandler>();

        return builder;
    }

    private static void AddPermissionPolicy(
        AuthorizationOptions options,
        string policyName,
        string resource,
        string action)
    {
        options.AddPolicy(
            policyName,
            policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(
                    new GatewayPermissionRequirement(resource, action));
            });
    }
}
