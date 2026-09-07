using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Models;
using CourseLibrary.Models.Course;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Courses.Search;

public sealed class SearchCourseQueryHandler(
    ICourseRepository repository,
    ILogger<SearchCourseQueryHandler> logger)
    : IHandler<SearchCourseQuery, PageResult<CourseResponse>>
{
    public async Task<PageResult<CourseResponse>> HandleAsync(
        SearchCourseQuery query,
        CancellationToken ct)
    {
        logger.SearchingCourses(query.SearchTerm, query.AuthorId, query.IncludeDeleted, query.IncludeRetired);
        var page = await repository.SearchAsync(
            new CourseSearchCriteria(
                query.SearchTerm,
                query.AuthorId,
                query.IncludeDeleted,
                query.IncludeRetired),
            query.PageSize,
            query.PageToken,
            ct);

        logger.CoursesSearched(page.Items.Count);
        return page.Map(course => CourseMapper.ToResponse(course));
    }
}