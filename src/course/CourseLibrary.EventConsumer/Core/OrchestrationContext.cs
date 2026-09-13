using System.Diagnostics;

namespace CourseLibrary.EventConsumer.Core;

internal class OrchestrationContext
{
    public byte[] Event { get; init; } = default!;
    public string InstanceId { get; init; } = default!;
    public string OrchestrationName { get; init; } = default!;
    public DateTimeOffset StartTime { get; init; } = default!;
    public bool IsReplaying { get; init; } = default!;
    public string? ParentTraceParent { get; init; }
    public string? ParentTraceState { get; init; }
}