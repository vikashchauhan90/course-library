using CourseLibrary.Platform.Messaging.Abstractions;

namespace CourseLibrary.Course.EventConsumer.Core;

internal sealed class OrchestrationContext<T>
{
    public required T Event { get; init; }
    public required string MessageId { get; init; }
    public required string InstanceId { get; init; }
    public required string OrchestrationName { get; init; }
    public required DateTimeOffset StartTime { get; init; }
    public required bool IsReplaying { get; init; }
    public required EventMetadata Metadata { get; init; }
}
