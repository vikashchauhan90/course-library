using CourseLibrary.Models;
using CourseLibrary.Models.Course;
using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Courses.Search;

public sealed record SearchCourseQuery(
    string? SearchTerm,
    string? AuthorId,
    bool IncludeDeleted,
    bool IncludeRetired,
    int PageSize,
    string? PageToken) : IQuery<PageResult<CourseResponse>>;