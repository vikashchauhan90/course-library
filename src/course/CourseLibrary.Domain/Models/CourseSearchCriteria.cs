namespace CourseLibrary.Domain.Models;

public sealed record CourseSearchCriteria(
    string? SearchTerm,
    string? AuthorId,
    bool IncludeDeleted,
    bool IncludeRetired);