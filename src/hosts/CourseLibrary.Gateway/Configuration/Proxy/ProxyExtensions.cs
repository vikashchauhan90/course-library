using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;

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
            options.ForwardLimit = 2;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return builder;
    }

    public static WebApplication UseGatewayProxy(this WebApplication app)
    {
        app.UseForwardedHeaders();
        app.UseHttpsRedirection();
        return app;
    }
}
