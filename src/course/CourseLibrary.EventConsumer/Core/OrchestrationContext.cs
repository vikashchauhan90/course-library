using CourseLibrary.Infrastructure.Configuration.Messaging;
using System.Diagnostics;

namespace CourseLibrary.EventConsumer.Core;

internal class OrchestrationContext<T>
{
    public T Event { get; init; } = default!;
    public string MessageId { get; init; } = string.Empty;
    public string InstanceId { get; init; } = default!;
    public string OrchestrationName { get; init; } = default!;
    public DateTimeOffset StartTime { get; init; } = default!;
    public bool IsReplaying { get; init; } = default!;
    public EventMetadata Metadata { get; init; } = default!;
}