namespace CourseLibrary.Gateway.Configuration.Security.Middlewares;

public sealed class ResponseHeaderCleanupMiddleware(RequestDelegate next)
{
    private const string XPoweredByHeader = "X-Powered-By";
    private const string ServerHeader = "Server";

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Remove(XPoweredByHeader);
            context.Response.Headers.Remove(ServerHeader);

            return Task.CompletedTask;
        });

        await next(context);
    }
}
