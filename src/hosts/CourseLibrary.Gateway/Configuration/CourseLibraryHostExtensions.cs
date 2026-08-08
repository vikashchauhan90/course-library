using CourseLibrary.Gateway.Configuration.Authentication;
using CourseLibrary.Gateway.Configuration.Authorization;
using CourseLibrary.Gateway.Configuration.Cors;
using CourseLibrary.Gateway.Configuration.Observability;
using CourseLibrary.Gateway.Configuration.Proxy;
using CourseLibrary.Gateway.Configuration.RateLimiting;
using System.Diagnostics;

namespace CourseLibrary.Gateway.Configuration;

internal static class CourseLibraryHostExtensions
{
    private const string AuthorizationPolicyName = "ApiAccess";

    public static WebApplicationBuilder AddCourseLibraryServices(
        this WebApplicationBuilder builder)
    {
        // W3C distributed tracing format.
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;
        Activity.ForceDefaultIdFormat = true;

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
        app.UseForwardedHeaders();
        app.UseHttpsRedirection();
        app.UseHttpLogging();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();


        var rateLimitOptions =
    app.Configuration
        .GetSection("RateLimiting")
        .Get<GatewayRateLimitingOptions>()!;

        app.MapReverseProxy()
            .RequireAuthorization(AuthorizationPolicyName)
            .RequireRateLimiting(rateLimitOptions.Ip.Name)
            .RequireRateLimiting(rateLimitOptions.User.Name)
            .RequireRateLimiting(rateLimitOptions.Concurrency.Name);

        return app;
    }

}