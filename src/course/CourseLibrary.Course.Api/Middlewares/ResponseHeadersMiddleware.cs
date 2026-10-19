using System.Diagnostics;

namespace CourseLibrary.Course.Api.Middlewares;

internal sealed class ResponseHeadersMiddleware(
    RequestDelegate next,
    ILogger<ResponseHeadersMiddleware> logger)
{
    public async Task InvokeAsync(
        HttpContext context)
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