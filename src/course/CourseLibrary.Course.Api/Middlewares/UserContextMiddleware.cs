using System.Diagnostics;
using CourseLibrary.Course.Api.Observability;

namespace CourseLibrary.Course.Api.Middlewares;

internal sealed class UserContextMiddleware(
    RequestDelegate next,
    ILogger<UserContextMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.Request.Headers["X-User-Id"].ToString();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await next(context);
            return;
        }

        using var scope = logger.BeginScope(
            new Dictionary<string, object?>
            {
                ["user.id"] = userId
            });

        Activity.Current?.SetTag("user.id", userId);
        logger.UserContextAdded();

        await next(context);
    }
}
