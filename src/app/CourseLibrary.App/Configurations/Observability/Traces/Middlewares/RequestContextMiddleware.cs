using CourseLibrary.App.Configuration.Observability;
using Microsoft.Extensions.Primitives;
using System.Diagnostics;

namespace CourseLibrary.App.Configuration.Observability.Traces.Middlewares;

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
            logger.LogWarning("Current activity is null. Starting a new activity for request context.");
            activity = Traces.ActivitySources.App
                .StartActivity(activityName);

            activity ??= new Activity(activityName).Start();
        }

        var correlationId = ResolveCorrelationId(context, activity);
        var traceParent = ResolveTraceParent(context, activity);
        var traceState = ResolveTraceState(context, activity);

        var requestId = context.TraceIdentifier;
        SetRequestHeader(context, TraceHeaders.CorrelationId, correlationId);
        SetRequestHeader(context, TraceHeaders.RequestId, requestId);
        SetRequestHeader(context, TraceHeaders.TraceParent, traceParent);
        if (!string.IsNullOrWhiteSpace(traceState))
            SetRequestHeader(context, TraceHeaders.TraceState, traceState);

        if (IsWriteRequest(context.Request) &&
            !context.Request.Headers.ContainsKey(TraceHeaders.IdempotencyKey))
        {
            SetRequestHeader(
                context,
                TraceHeaders.IdempotencyKey,
                Guid.NewGuid().ToString("N"));
        }

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

        Traces.RequestContextActivityTags.Apply(
            activity,
            context,
            correlationId,
            requestId,
            route);

        try
        {
            await next(context);
        }
        finally
        {
            Traces.RequestContextActivityTags.ApplyResponse(activity, context);
        }
    }

    private string ResolveCorrelationId(
        HttpContext context,
        Activity? activity)
    {
        if (context.Request.Headers.TryGetValue(
                TraceHeaders.CorrelationId,
                out var correlationId)
            && !StringValues.IsNullOrEmpty(correlationId))
        {
            return correlationId.ToString();
        }

        logger.LogWarning("Correlation ID not found in request headers. Generating a new correlation ID.");
        // Correlation ID has its own fallback.
        // It must NOT fall back to traceparent or tracestate.
        return Traces.TracingHelper.GenerateTraceIdentifier(activity);
    }

    private string ResolveTraceParent(
        HttpContext context,
        Activity? activity)
    {
        if (context.Request.Headers.TryGetValue(
                Traces.TraceHeaders.TraceParent,
                out var traceParent)
            && !StringValues.IsNullOrEmpty(traceParent))
        {
            return traceParent.ToString();
        }

        logger.LogWarning("Traceparent not found in request headers. Generating a new traceparent.");

        var traceparent = Traces.TracingHelper.GenerateTraceParent(activity);
        context.Request.Headers[Traces.TraceHeaders.TraceParent] = traceparent;
        return traceparent;
    }

    private string ResolveTraceState(
        HttpContext context,
        Activity? activity)
    {
        if (context.Request.Headers.TryGetValue(
                Traces.TraceHeaders.TraceState,
                out var traceState)
            && !StringValues.IsNullOrEmpty(traceState))
        {
            return traceState.ToString();
        }

        logger.LogWarning("Tracestate not found in request headers. Generating a new tracestate.");

        var tracestate = Traces.TracingHelper.GenerateTraceState(activity);
        context.Request.Headers[Traces.TraceHeaders.TraceState] = tracestate;
        return tracestate;
    }

    private static string GetSafeRequestPath(HttpRequest request)
    {
        var path = request.Path.Value;

        return string.IsNullOrWhiteSpace(path)
            ? "/"
            : path;
    }

    private static bool IsWriteRequest(HttpRequest request) =>
        HttpMethods.IsPost(request.Method) ||
        HttpMethods.IsPut(request.Method) ||
        HttpMethods.IsPatch(request.Method) ||
        HttpMethods.IsDelete(request.Method);

    private static void SetRequestHeader(
        HttpContext context,
        string name,
        string value) =>
        context.Request.Headers[name] = value;
}
