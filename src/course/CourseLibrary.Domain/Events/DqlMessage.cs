using CourseLibrary.Domain.ValueObjects;

namespace CourseLibrary.Domain.Events;

public sealed class DqlMessage
{
    public required string Id { get; init; }

    public required string EventType { get; init; }

    public required string MessageId { get; init; }

    public string? CorrelationId { get; init; }

    public string? TraceParent { get; init; }

    public string? TraceState { get; init; }

    public string? Subject { get; init; }

    public string? DeadLetterReason { get; init; }

    public string? DeadLetterErrorDescription { get; init; }

    public DateTimeOffset EnqueuedTime { get; init; }

    public DateTimeOffset DeadLetteredAt { get; init; }

    public int DeliveryCount { get; init; }

    public DqlMessageStatus Status { get; set; }

    public DateTimeOffset? ReplayedAt { get; set; }

    public string? ReplayedMessageId { get; set; }

    public string? LastError { get; set; }
    public string Payload { get; set; } = string.Empty;
}
