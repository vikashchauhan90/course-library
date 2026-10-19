namespace CourseLibrary.Course.Api.Middlewares;

internal static class MiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(
        this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }

    public static IApplicationBuilder UseRequestContext(
        this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<RequestContextMiddleware>();
    }

    public static IApplicationBuilder UseUserContext(
        this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<UserContextMiddleware>();
    }

    public static IApplicationBuilder UseRequestMetrics(
        this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<RequestMetricsMiddleware>();
    }

    public static IApplicationBuilder UseResponseHeaders(
        this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<ResponseHeadersMiddleware>();
    }


    public static IApplicationBuilder UseCourseApiTelemetry(
        this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<CourseApiTelemetryMiddleware>();
    }
}
