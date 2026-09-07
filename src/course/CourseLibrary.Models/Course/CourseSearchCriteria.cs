namespace CourseLibrary.Models.Course;

/// <summary>
/// Represents the criteria used to search for courses in the course library.
/// </summary>
/// <param name="SearchTerm">The term to search for in the course titles and descriptions.</param>
/// <param name="AuthorId">The ID of the author whose courses to include.</param>
/// <param name="IncludeDeleted">Indicates whether to include deleted courses.</param>
/// <param name="IncludeRetired">Indicates whether to include retired courses.</param>
public sealed record CourseSearchCriteria(
    string? SearchTerm,
    string? AuthorId,
    bool IncludeDeleted,
    bool IncludeRetired);
