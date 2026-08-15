namespace CourseLibrary.Idp.Domain.Models;

public sealed record PageResult<T>(
    IReadOnlyList<T> Items,
    string? ContinuationToken,
    bool HasMore);