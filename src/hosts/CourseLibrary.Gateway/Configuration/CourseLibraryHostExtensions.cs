using CourseLibrary.Gateway.Configuration.Authentication;
using CourseLibrary.Gateway.Configuration.Authorization;
using CourseLibrary.Gateway.Configuration.Cors;
using CourseLibrary.Gateway.Configuration.Observability;
using CourseLibrary.Gateway.Configuration.Observability.Logs;
using CourseLibrary.Gateway.Configuration.Proxy;
using CourseLibrary.Gateway.Configuration.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using System.Diagnostics;
using CourseLibrary.Infrastructure.Resilience;

namespace CourseLibrary.Gateway.Configuration;

internal static class CourseLibraryHostExtensions
{
    private const string CorsPolicyName = "GatewayCors";
    private const string AuthorizationPolicyName = "ApiAccess";
    private const string IpPolicyName = "IpRateLimit";
    private const string UserPolicyName = "UserRateLimit";
    private const string ConcurrentPolicyName = "ConcurrentRequestLimit";

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

        if (builder.Configuration.GetValue<bool>("Observability:Enabled", true))
        {
            builder.AddObservability();
        }

        builder.AddGatewayRateLimiting();

        builder.Services.AddCourseLibraryHttpResilience(builder.Configuration);
        builder.Services.AddCourseLibraryResilience();
        builder.Services.AddSingleton<PolicyFactory>();

        return builder;
    }

    public static WebApplication UseCourseLibraryPipeline(
        this WebApplication app)
    {
        app.UseGatewayProxy();
        app.UseHttpLogging();
        app.UseRouting();
        app.UseCors(CorsPolicyName);
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();

        app.MapReverseProxy()
            .RequireAuthorization(AuthorizationPolicyName)
            .RequireRateLimiting(IpPolicyName)
            .RequireRateLimiting(UserPolicyName)
            .RequireRateLimiting(ConcurrentPolicyName);

        return app;
    }

}