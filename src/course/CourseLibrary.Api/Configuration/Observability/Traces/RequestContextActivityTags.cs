using System.Diagnostics;

namespace CourseLibrary.Api.Configuration.Observability.Traces;

internal static class RequestContextActivityTags
{
    public static void Apply(
        Activity? activity,
        HttpContext context,
        string correlationId,
        string requestId,
        string route)
    {
        if (activity is null)
        {
            return;
        }

        // Request identity
        activity.SetTag("request.id", requestId);
        activity.SetTag("request.correlation_id", correlationId);

        // HTTP request
        activity.SetTag("http.request.method", context.Request.Method);
        activity.SetTag("url.path", context.Request.Path.Value);
        activity.SetTag("url.scheme", context.Request.Scheme);
        activity.SetTag("server.address", context.Request.Host.Host);

        if (context.Request.Host.Port.HasValue)
        {
            activity.SetTag("server.port", context.Request.Host.Port.Value);
        }

        // Routing
        activity.SetTag("http.route", route);

        // User agent
        var userAgent = context.Request.Headers["User-Agent"].ToString();

        if (!string.IsNullOrWhiteSpace(userAgent))
        {
            activity.SetTag("user_agent.original", userAgent);
        }

        // Network information
        var remoteIp = context.Connection.RemoteIpAddress?.ToString();

        if (!string.IsNullOrWhiteSpace(remoteIp))
        {
            activity.SetTag("client.address", remoteIp);
        }

        // Request content
        if (!string.IsNullOrWhiteSpace(context.Request.ContentType))
        {
            activity.SetTag(
                "http.request.header.content_type",
                context.Request.ContentType);
        }
    }

    public static void ApplyResponse(
        Activity? activity,
        HttpContext context)
    {
        if (activity is null)
        {
            return;
        }

        var statusCode = context.Response.StatusCode;

        activity.SetTag(
            "http.response.status_code",
            statusCode);

        // Optional response size information.
        if (context.Response.ContentLength.HasValue)
        {
            activity.SetTag(
                "http.response.body.size",
                context.Response.ContentLength.Value);
        }

        // Mark Activity status based on HTTP response.
        if (statusCode >= 500)
        {
            activity.SetStatus(
                ActivityStatusCode.Error,
                $"HTTP {statusCode}");
        }
        else
        {
            activity.SetStatus(ActivityStatusCode.Unset);
        }
    }
    public static void ApplyUserId(
        Activity? activity,
        string userId)
    {
        if (activity is null || string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        activity.SetTag("user.id", userId);
    }


}
