using CourseLibrary.Gateway.Configuration.Observability.Logs.Middlewares;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CourseLibrary.Gateway.Configuration.Observability.Logs;

internal static class LoggingExtensions
{
    public static WebApplicationBuilder AddLoggingObservability(
       this WebApplicationBuilder builder)
    {
        builder.Logging.EnableRedaction();

        return builder;
    }

    public static IApplicationBuilder UseRequestContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestContextMiddleware>();
    }

    public static IApplicationBuilder UseUserContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<UserContextMiddleware>();
    }
}