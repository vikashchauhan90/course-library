using System.Diagnostics;
using CourseLibrary.Course.Api.Observability;
using CourseLibrary.Platform.Observability.Traces;

namespace CourseLibrary.Course.Api.Middlewares;

internal sealed class ResponseHeadersMiddleware(
    RequestDelegate next,
    ILogger<ResponseHeadersMiddleware> logger)
{
    public Task InvokeAsync(HttpContext context)
    {
        var activity = Activity.Current;
        var correlationId = context.Request.Headers[TraceHeaders.CorrelationId].ToString();
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = activity?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        }

        var requestId = context.TraceIdentifier;
        var traceParent = activity?.Id;
        var traceState = activity?.TraceStateString;

        context.Response.OnStarting(() =>
        {
            SetHeaderIfMissing(
                context.Response.Headers,
                TraceHeaders.CorrelationId,
                correlationId);
            SetHeaderIfMissing(
                context.Response.Headers,
                TraceHeaders.RequestId,
                requestId);

            if (!string.IsNullOrWhiteSpace(traceParent))
            {
                SetHeaderIfMissing(
                    context.Response.Headers,
                    TraceHeaders.TraceParent,
                    traceParent);
            }

            if (!string.IsNullOrWhiteSpace(traceState))
            {
                SetHeaderIfMissing(
                    context.Response.Headers,
                    TraceHeaders.TraceState,
                    traceState);
            }

            logger.ResponseHeadersWritten(
                requestId,
                correlationId,
                activity?.TraceId.ToString());

            return Task.CompletedTask;
        });

        return next(context);
    }

    private static void SetHeaderIfMissing(
        IHeaderDictionary headers,
        string name,
        string value)
    {
        if (!headers.ContainsKey(name))
        {
            headers[name] = value;
        }
    }
}
