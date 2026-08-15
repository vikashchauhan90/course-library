using System.Diagnostics;

namespace CourseLibrary.Api.Configuration.Observability.Traces.Middlewares;

internal sealed class UserContextMiddleware(
    RequestDelegate next,
    ILogger<UserContextMiddleware> logger)
{
    private const string activityName = "course.library.request.context";
    public async Task InvokeAsync(HttpContext context)
    {
        var activity = Activity.Current;

        if (activity is null)
        {
            logger.LogWarning("Current activity is null. Starting a new activity for request context.");
            activity = ActivitySources.Api
                .StartActivity(activityName);

            activity ??= new Activity(activityName).Start();
        }

        var userId = context.Request.Headers["X-User-Id"].ToString();

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
}