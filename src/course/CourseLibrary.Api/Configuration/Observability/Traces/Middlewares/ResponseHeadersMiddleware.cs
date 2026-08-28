using CourseLibrary.Infrastructure.OutputCache;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Primitives;
using System.Diagnostics;
using System.Globalization;
using InfrastructureObservability = CourseLibrary.Infrastructure.Observability;
using InfraTrace = CourseLibrary.Infrastructure.Observability.Traces;

namespace CourseLibrary.Api.Configuration.Observability.Traces.Middlewares;

internal sealed class ResponseHeadersMiddleware(
    RequestDelegate next,
    ILogger<ResponseHeadersMiddleware> logger)
{
    public async Task InvokeAsync(
        HttpContext context,
         IOutputCacheDiagnostics outputCache)
    {
        context.Response.OnStarting(
            () =>
            {
                var activity = Activity.Current;
                var correlationId = ResolveCorrelationId(context, activity);
                var traceParent = ResolveTraceParent(context, activity);
                var traceState = ResolveTraceState(context, activity);
                var requestId = context.TraceIdentifier;

                // Correlation ID is application-specific and independent
                // from traceparent/tracestate.
                if (!context.Response.Headers.ContainsKey(correlationId))
                {
                    logger.LogDebug(
                        "Adding X-Correlation-ID header to response: {CorrelationId}",
                        correlationId);

                    context.Response.Headers[InfraTrace.TraceHeaders.CorrelationId] = correlationId;
                }

                // Request ID is application-specific and independent
                if (!context.Response.Headers.ContainsKey(InfraTrace.TraceHeaders.RequestId))
                {
                    logger.LogDebug(
                        "Adding X-Request-ID header to response: {RequestId}",
                        requestId);

                    context.Response.Headers[InfraTrace.TraceHeaders.RequestId] = requestId;
                }

                if (!context.Response.Headers.ContainsKey(InfraTrace.TraceHeaders.TraceParent))
                {
                    logger.LogDebug(
                        "Adding traceparent header to response: {TraceParent}",
                        traceParent);
                    context.Response.Headers[InfraTrace.TraceHeaders.TraceParent] = traceParent;

                }

                if (!context.Response.Headers.ContainsKey(InfraTrace.TraceHeaders.TraceId))
                {
                    logger.LogDebug(
                        "Adding traceId header to response: {TraceId}",
                        activity?.TraceId.ToString());

                    context.Response.Headers[InfraTrace.TraceHeaders.TraceId] = activity?.TraceId.ToString();

                }

                foreach (var bag in activity?.Baggage ?? Array.Empty<KeyValuePair<string, string?>>())
                {
                    context.Response.Headers.Append(InfraTrace.TraceHeaders.Baggage, $"{bag.Key}={bag.Value}");
                }

                if (outputCache.Hit.HasValue)
                {
                    context.Response.Headers[
                        InfraTrace.TraceHeaders.CacheHit] =
                        outputCache.Hit.Value.ToString()
                            .ToLowerInvariant();
                }

                if (outputCache.ExpirationDuration.HasValue)
                {
                    context.Response.Headers[
                        InfraTrace.TraceHeaders.CacheTtl] =
                        outputCache.ExpirationDuration.Value
                            .TotalSeconds
                            .ToString(CultureInfo.InvariantCulture);
                }

                return Task.CompletedTask;
            });
        await next(context);
    }

    private string ResolveCorrelationId(
     HttpContext context,
     Activity? activity)
    {
        if (context.Request.Headers.TryGetValue(
                InfraTrace.TraceHeaders.CorrelationId,
                out var correlationId)
            && !StringValues.IsNullOrEmpty(correlationId))
        {
            return correlationId.ToString();
        }

        logger.LogWarning("Correlation ID not found in request headers. Generating a new correlation ID.");
        // Correlation ID has its own fallback.
        // It must NOT fall back to traceparent or tracestate.
        return InfrastructureObservability.Traces.TracingHelper.GenerateTraceIdentifier(activity);
    }
    private string ResolveTraceParent(
    HttpContext context,
    Activity? activity)
    {
        if (context.Request.Headers.TryGetValue(
                InfraTrace.TraceHeaders.TraceParent,
                out var traceParent)
            && !StringValues.IsNullOrEmpty(traceParent))
        {
            return traceParent.ToString();
        }

        logger.LogWarning("Traceparent not found in request headers. Generating a new traceparent.");

        var traceparent = InfrastructureObservability.Traces.TracingHelper.GenerateTraceParent(activity);
        context.Request.Headers[InfraTrace.TraceHeaders.TraceParent] = traceparent;
        return traceparent;
    }

    private string ResolveTraceState(
        HttpContext context,
        Activity? activity)
    {
        if (context.Request.Headers.TryGetValue(
                InfraTrace.TraceHeaders.TraceState,
                out var traceState)
            && !StringValues.IsNullOrEmpty(traceState))
        {
            return traceState.ToString();
        }

        logger.LogWarning("Tracestate not found in request headers. Generating a new tracestate.");

        var tracestate = InfrastructureObservability.Traces.TracingHelper.GenerateTraceState(activity);
        context.Request.Headers[InfraTrace.TraceHeaders.TraceState] = tracestate;
        return tracestate;
    }
}
