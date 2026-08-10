using CourseLibrary.Api.Configuration.Observability.Metrics.Middlewares;

namespace CourseLibrary.Api.Configuration.Observability.Metrics;

public static class MetersExtensions
{
    public static IApplicationBuilder UseRequestMetrics(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestMetricsMiddleware>();
    }
}
