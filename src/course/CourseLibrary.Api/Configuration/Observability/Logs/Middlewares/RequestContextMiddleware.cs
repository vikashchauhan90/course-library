using ApiObservability = CourseLibrary.Api.Configuration.Observability;
using InfrastructureObservability = CourseLibrary.Infrastructure.Observability;
using Microsoft.Extensions.Primitives;
using System.Diagnostics;

namespace CourseLibrary.Api.Configuration.Observability.Logs.Middlewares;

internal sealed class RequestContextMiddleware(
    RequestDelegate next,
    ILogger<RequestContextMiddleware> logger)
{
    private const string activityName = "course.library.request.context";
    public async Task InvokeAsync(HttpContext context)
    {
        var activity = Activity.Current;

        if (activity is null)
        {
            activity = ApiObservability.Traces.ActivitySources.Api
                .StartActivity(activityName);

            activity ??= new Activity(activityName).Start();
        }

        var correlationId = ResolveCorrelationId(context, activity);
        var traceParent = ResolveTraceParent(context, activity);
        var traceState = ResolveTraceState(context, activity);

        var requestId = context.TraceIdentifier;
        var safePath = GetSafeRequestPath(context.Request);
        var route = safePath;

        var scopeValues = new Dictionary<string, object?>
        {
            ["request.id"] = requestId,
            ["request.correlation_id"] = correlationId,

            ["request.method"] = context.Request.Method,
            ["request.path"] = safePath,
            ["request.host"] = context.Request.Host.Value,
            ["request.scheme"] = context.Request.Scheme,
            ["request.route"] = route,
            ["request.user_agent"] = context.Request.Headers["User-Agent"].ToString(),
            ["request.remote_ip"] = context.Connection.RemoteIpAddress?.ToString(),
            ["request.content_type"] = context.Request.ContentType?.ToString(),

            ["trace.id"] = activity?.TraceId.ToString(),
            ["trace.parent_id"] = traceParent,
            ["trace.state"] = traceState
        };

        using var scope = logger.BeginScope(scopeValues);

        ApiObservability.Traces.RequestContextActivityTags.Apply(
            activity,
            context,
            correlationId,
            requestId,
            route);

        // Correlation ID is application-specific and independent
        // from traceparent/tracestate.
        if (!context.Response.Headers.ContainsKey("X-Correlation-ID"))
        {
            context.Response.Headers["X-Correlation-ID"] = correlationId;
        }

        try
        {
            await next(context);
        }
        finally
        {
            ApiObservability.Traces.RequestContextActivityTags.ApplyResponse(activity, context);
        }
    }

    private static string ResolveCorrelationId(
        HttpContext context,
        Activity? activity)
    {
        if (context.Request.Headers.TryGetValue(
                "X-Correlation-ID",
                out var correlationId)
            && !StringValues.IsNullOrEmpty(correlationId))
        {
            return correlationId.ToString();
        }

        // Correlation ID has its own fallback.
        // It must NOT fall back to traceparent or tracestate.
        return InfrastructureObservability.Traces.TracingHelper.GenerateTraceIdentifier(activity);
    }

    private static string ResolveTraceParent(
        HttpContext context,
        Activity? activity)
    {
        if (context.Request.Headers.TryGetValue(
                "traceparent",
                out var traceParent)
            && !StringValues.IsNullOrEmpty(traceParent))
        {
            return traceParent.ToString();
        }

        return InfrastructureObservability.Traces.TracingHelper.GenerateTraceParent(activity);
    }

    private static string ResolveTraceState(
        HttpContext context,
        Activity? activity)
    {
        if (context.Request.Headers.TryGetValue(
                "tracestate",
                out var traceState)
            && !StringValues.IsNullOrEmpty(traceState))
        {
            return traceState.ToString();
        }

        return InfrastructureObservability.Traces.TracingHelper.GenerateTraceState(activity);
    }

    private static string GetSafeRequestPath(HttpRequest request)
    {
        var path = request.Path.Value;

        return string.IsNullOrWhiteSpace(path)
            ? "/"
            : path;
    }
}
