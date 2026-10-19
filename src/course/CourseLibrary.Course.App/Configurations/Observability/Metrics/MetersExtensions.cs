using CourseLibrary.Course.App.Configuration.Observability.Metrics.Middlewares;

namespace CourseLibrary.Course.App.Configuration.Observability.Metrics;

public static class MetersExtensions
{
    public static IApplicationBuilder UseRequestMetrics(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestMetricsMiddleware>();
    }
}
