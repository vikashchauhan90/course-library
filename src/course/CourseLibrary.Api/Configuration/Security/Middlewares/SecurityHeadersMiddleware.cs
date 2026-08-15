namespace CourseLibrary.Api.Configuration.Security.Middlewares;

public sealed class SecurityHeadersMiddleware(
    RequestDelegate next,
    ILogger<SecurityHeadersMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            logger.LogDebug(
                "Response starting: {StatusCode}",
                context.Response.StatusCode);

            var headers = context.Response.Headers;

            headers.TryAdd("X-Content-Type-Options", "nosniff");
            headers.TryAdd("X-Frame-Options", "DENY");
            headers.TryAdd("Referrer-Policy", "no-referrer");

            return Task.CompletedTask;
        });
        context.Response.OnCompleted(() =>
        {
            logger.LogDebug(
                "Response completed: {StatusCode}",
                context.Response.StatusCode);

            return Task.CompletedTask;
        });
        await next(context);
    }
}