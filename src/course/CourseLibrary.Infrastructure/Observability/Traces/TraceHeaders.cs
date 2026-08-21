namespace CourseLibrary.Infrastructure.Observability.Traces;

public static class TraceHeaders
{
    /// <summary>
    /// Application correlation identifier (custom header).
    /// </summary>
    public const string CorrelationId = "X-Correlation-ID";

    /// <summary>
    /// Unique request identifier (custom header).
    /// </summary>
    public const string RequestId = "X-Request-ID";

    /// <summary>
    /// W3C Trace Context parent identifier.
    /// RFC: lowercase header name.
    /// </summary>
    public const string TraceParent = "traceparent";

    /// <summary>
    /// W3C Trace Context vendor-specific state.
    /// RFC: lowercase header name.
    /// </summary>
    public const string TraceState = "tracestate";

    /// <summary>
    /// W3C Trace Context baggage for distributed context propagation.
    /// </summary>
    public const string Baggage = "baggage";

    /// <summary>
    /// Unique trace identifier (custom header).
    /// </summary>
    public const string TraceId = "X-Trace-ID";
}