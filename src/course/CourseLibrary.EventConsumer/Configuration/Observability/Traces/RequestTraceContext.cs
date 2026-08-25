using System.Diagnostics;

namespace CourseLibrary.EventConsumer.Configuration.Observability.Traces;

internal class RequestTraceContext
{
    public string TraceId { get; set; } = string.Empty;
    public string TraceParent { get; set; } = string.Empty;
    public string ParentSpanId { get; set; } = string.Empty;
    public string TraceState { get; set; } = string.Empty;

    public ActivityKind ActivityKind { get; set; }

    public string ActivityId { get; set; } = string.Empty;
    public string ActivityParentId { get; set; } = string.Empty;
    public string ActivityState { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public string ActivityTypeName { get; set; } = string.Empty;
}