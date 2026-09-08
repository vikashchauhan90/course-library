namespace CourseLibrary.Gateway.Configuration.Security.Middlewares;

public sealed class ResponseHeaderCleanupMiddleware(RequestDelegate next)
{
    private const string XPoweredByHeader = "X-Powered-By";
    private const string ServerHeader = "Server";
    private const string XUserIdHeader = "X-User-Id";
    private const string XUserEmailHeader = "X-User-Email";
    private const string XUserNameHeader = "X-User-Name";
    private const string XClientIdHeader = "X-Client-Id";
    private const string XIdentityTypeHeader = "X-Identity-Type";

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Remove(XPoweredByHeader);
            context.Response.Headers.Remove(ServerHeader);
            context.Response.Headers.Remove(XUserIdHeader);
            context.Response.Headers.Remove(XUserEmailHeader);
            context.Response.Headers.Remove(XUserNameHeader);
            context.Response.Headers.Remove(XClientIdHeader);
            context.Response.Headers.Remove(XIdentityTypeHeader);

            return Task.CompletedTask;
        });

        await next(context);
    }
}
