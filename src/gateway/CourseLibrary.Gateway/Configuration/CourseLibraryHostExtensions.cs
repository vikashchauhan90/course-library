using CourseLibrary.Gateway.Configuration.Authentication;
using CourseLibrary.Gateway.Configuration.Authorization;
using CourseLibrary.Gateway.Configuration.Cors;
using CourseLibrary.Gateway.Configuration.Exceptions;
using CourseLibrary.Gateway.Configuration.Observability;
using CourseLibrary.Gateway.Configuration.Observability.Logs;
using CourseLibrary.Gateway.Configuration.Proxy;
using CourseLibrary.Gateway.Configuration.RateLimiting;
using CourseLibrary.Gateway.Configuration.Security;
using System.Diagnostics;

namespace CourseLibrary.Gateway.Configuration;

internal static class CourseLibraryHostExtensions
{

    public static WebApplicationBuilder AddCourseLibraryServices(
        this WebApplicationBuilder builder)
    {
        // W3C distributed tracing format.
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;
        Activity.ForceDefaultIdFormat = true;

        // Kestrel configuration.
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.AddServerHeader = false;
        });

        // .NET framework services.
        builder.Services.AddOptions();
        builder.Services.AddHttpClient();
        builder.Services.AddHttpContextAccessor();

        builder.AddGatewayProxy();
        builder.AddGatewayAuthentication();
        builder.AddGatewayAuthorization();
        builder.AddGatewayCors();

        // HTTP logging.
        builder.Services.AddHttpLogging();

        builder.Services.AddServiceDiscovery();

        builder.Services
            .AddReverseProxy()
            .AddServiceDiscoveryDestinationResolver()
            .LoadFromConfig(
                builder.Configuration.GetSection("ReverseProxy"));

        builder.AddObservability();

        builder.AddGatewayRateLimiting();

        return builder;
    }

    public static WebApplication UseCourseLibraryPipeline(
        this WebApplication app)
    {
        app.UseGlobalExceptionHandler();
        app.UseForwardedHeaders();
        app.UseResponseHeaderCleanup();
        app.UseSecurityHeaders(
            SecurityHeadersPolicies.AddCourseLibraryDefaultSecurityHeaders());
        app.UseRequestContext();
        app.UseHttpsRedirection();
        app.UseHttpLogging();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseUserContext();
        app.UseAuthorization();
        app.UseUserIdentityForwarding();
        app.UseRateLimiter();


        var rateLimitOptions =
    app.Configuration
        .GetSection("RateLimiting")
        .Get<GatewayRateLimitingOptions>()!;

        app.MapReverseProxy()
            .RequireAuthorization(GatewayAuthorizationConstants.UserOrM2MPolicy)
            .RequireRateLimiting(rateLimitOptions.Ip.Name)
            .RequireRateLimiting(rateLimitOptions.User.Name)
            .RequireRateLimiting(rateLimitOptions.Concurrency.Name);

        return app;
    }

}