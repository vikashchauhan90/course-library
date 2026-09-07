namespace CourseLibrary.Client;

public sealed record CourseApiProblemDetails(
    string? Type,
    int? Status,
    string? Title,
    string? Detail,
    string? Instance,
    string? TraceId,
    DateTimeOffset? Timestamp,
    IReadOnlyDictionary<string, string[]>? Errors);