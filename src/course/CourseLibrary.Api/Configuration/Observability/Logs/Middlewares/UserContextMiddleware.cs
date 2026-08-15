using CourseLibrary.Api.Configuration.Observability.Traces;
using System.Diagnostics;

namespace CourseLibrary.Api.Configuration.Observability.Logs.Middlewares;

internal sealed class UserContextMiddleware(
    RequestDelegate next,
    ILogger<UserContextMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var activity = Activity.Current ??
            ActivitySources.Api.StartActivity("course.library.user.context");

        var userId = ResolveUserId(context);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            using var scope = logger.BeginScope(
                new Dictionary<string, object?>
                {
                    ["user.id"] = userId
                });

            RequestContextActivityTags.ApplyUserId(
                activity,
                userId);

            await next(context);
            return;
        }

        await next(context);
    }

    private static string? ResolveUserId(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return context.User.FindFirst("sub")?.Value
            ?? context.User.FindFirst(
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                ?.Value;
    }
}