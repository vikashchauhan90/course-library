using System.Diagnostics;
using CourseLibrary.Course.Api.Observability;
using CourseLibrary.Platform.Observability.Traces;

namespace CourseLibrary.Course.Api.Middlewares;

internal sealed class RequestContextMiddleware(
    RequestDelegate next,
    ILogger<RequestContextMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        using var requestActivity = Activity.Current is null
            ? CourseApiActivitySource.Shared.StartActivity("http.request.context")
            : null;
        using var fallbackActivity = Activity.Current is null && requestActivity is null
            ? new Activity("http.request.context").Start()
            : null;

        var activity = Activity.Current ?? requestActivity ?? fallbackActivity;
        var correlationId = ResolveCorrelationId(context, activity);
        var requestId = context.TraceIdentifier;
        context.Request.Headers[TraceHeaders.CorrelationId] = correlationId;
        context.Request.Headers[TraceHeaders.RequestId] = requestId;
        if (!string.IsNullOrWhiteSpace(activity?.Id))
        {
            context.Request.Headers[TraceHeaders.TraceParent] = activity.Id;
        }

        if (!string.IsNullOrWhiteSpace(activity?.TraceStateString))
        {
            context.Request.Headers[TraceHeaders.TraceState] = activity.TraceStateString;
        }

        var requestPath = GetSafeRequestPath(context.Request);
        var route = context.GetEndpoint() is RouteEndpoint endpoint
            ? endpoint.RoutePattern.RawText ?? requestPath
            : requestPath;

        var scopeValues = new Dictionary<string, object?>
        {
            ["request.id"] = requestId,
            ["request.correlation_id"] = correlationId,
            ["request.method"] = context.Request.Method,
            ["request.path"] = requestPath,
            ["request.host"] = context.Request.Host.Value,
            ["request.scheme"] = context.Request.Scheme,
            ["request.route"] = route,
            ["request.user_agent"] = context.Request.Headers["User-Agent"].ToString(),
            ["request.remote_ip"] = context.Connection.RemoteIpAddress?.ToString(),
            ["request.content_type"] = context.Request.ContentType,
            ["trace.id"] = activity?.TraceId.ToString(),
            ["trace.parent_id"] = activity?.ParentId,
            ["trace.state"] = activity?.TraceStateString
        };

        using var scope = logger.BeginScope(scopeValues);

        if (activity is not null)
        {
            activity.SetTag("request.id", requestId);
            activity.SetTag("request.correlation_id", correlationId);
            activity.SetTag("http.request.method", context.Request.Method);
            activity.SetTag("url.path", requestPath);
            activity.SetTag("url.scheme", context.Request.Scheme);
            activity.SetTag("server.address", context.Request.Host.Host);
            activity.SetTag("http.route", route);

            var userAgent = context.Request.Headers["User-Agent"].ToString();
            if (!string.IsNullOrWhiteSpace(userAgent))
            {
                activity.SetTag("user_agent.original", userAgent);
            }

            var remoteAddress = context.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrWhiteSpace(remoteAddress))
            {
                activity.SetTag("client.address", remoteAddress);
            }
        }

        logger.RequestContextEstablished(requestId);

        try
        {
            await next(context);
        }
        finally
        {
            if (activity is not null)
            {
                activity.SetTag("http.response.status_code", context.Response.StatusCode);
                if (context.Response.StatusCode >= StatusCodes.Status500InternalServerError)
                {
                    activity.SetStatus(
                        ActivityStatusCode.Error,
                        $"HTTP {context.Response.StatusCode}");
                }
            }
        }
    }

    private static string ResolveCorrelationId(
        HttpContext context,
        Activity? activity)
    {
        var correlationId = context.Request.Headers[TraceHeaders.CorrelationId].ToString();
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId;
        }

        return activity?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
    }

    private static string GetSafeRequestPath(HttpRequest request)
    {
        var path = request.Path.Value;
        return string.IsNullOrWhiteSpace(path) ? "/" : path;
    }
}
