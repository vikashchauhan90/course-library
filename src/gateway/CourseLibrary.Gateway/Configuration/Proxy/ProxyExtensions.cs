using Microsoft.AspNetCore.HttpOverrides;

namespace CourseLibrary.Gateway.Configuration.Proxy;

internal static class GatewayProxyExtensions
{
    public static WebApplicationBuilder AddGatewayProxy(
        this WebApplicationBuilder builder)
    {
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                       ForwardedHeaders.XForwardedProto |
                                       ForwardedHeaders.XForwardedHost;
            // Keep ASP.NET Core's trusted-proxy defaults. Production ingress
            // addresses must be added explicitly rather than accepting
            // X-Forwarded-* values from arbitrary callers.
            options.ForwardLimit = 1;
        });

        return builder;
    }

}
