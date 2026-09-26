namespace CourseLibrary.Infrastructure.Configuration.Messaging;

public sealed record EventMetadata(
    string EventId,
    long EventOccurredAt,
    string EventType,
    string Destination,
    string? UserId,
    string Source,
    string? CorrelationId,
    string? TraceParent,
    string? TraceState);