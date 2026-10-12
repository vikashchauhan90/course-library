
namespace CourseLibrary.Messaging.Abstractions;

public sealed record ServiceBusEventPayload
{
    public required Guid EventId { get; init; }
    public required string EventType { get; init; } = string.Empty;
    public required DateTimeOffset OccurredAt { get; init; }
    public required BinaryData Payload { get; init; } = BinaryData.Empty;
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
    public int Version { get; init; } = 1;
    public string? Source { get; init; }
    public string? Subject { get; init; }
    public EventMetadata Metadata { get; init; } = new();
}
