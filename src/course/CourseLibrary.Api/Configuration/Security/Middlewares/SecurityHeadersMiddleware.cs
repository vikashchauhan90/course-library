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
            headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
            headers.TryAdd("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
            headers.TryAdd("Content-Security-Policy", "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data:; font-src 'self'; connect-src 'self'; object-src 'none'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'; upgrade-insecure-requests; block-all-mixed-content");
            headers.TryAdd("Cache-Control", "no-cache, no-store, must-revalidate");
            headers.TryAdd("Pragma", "no-cache");
            headers.TryAdd("Expires", "0");

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