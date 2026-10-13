using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Course.Contracts.Pagination;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Persistence.Abstractions.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.Operations.Courses;

namespace CourseLibrary.Course.Application.Operations.Courses.Search;

public interface ISearchCoursesOperation
{
    Task<Result<PageResult<CourseResponse>>> ExecuteAsync(
        CourseSearchCriteria? criteria,
        int pageSize = 20,
        string? continuationToken = null,
        CancellationToken cancellationToken = default);
}

internal sealed class SearchCoursesOperation(
    ICourseRepository repository,
    IValidator<SearchCoursesRequest> validator,
    ILogger<SearchCoursesOperation> logger) : ISearchCoursesOperation
{
    public Task<Result<PageResult<CourseResponse>>> ExecuteAsync(
        CourseSearchCriteria? criteria,
        int pageSize = 20,
        string? continuationToken = null,
        CancellationToken cancellationToken = default)
        => CourseApplicationOperation.ExecuteResultAsync(logger, "courses.search", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(new SearchCoursesRequest(pageSize), cancellationToken));
            var searchCriteria = criteria ?? new CourseSearchCriteria(null, null, false, false);
            logger.SearchingCourses(
                !string.IsNullOrWhiteSpace(searchCriteria.SearchTerm),
                searchCriteria.AuthorId,
                searchCriteria.IncludeDeleted,
                searchCriteria.IncludeRetired);
            var page = await repository.SearchAsync(searchCriteria, pageSize, continuationToken, cancellationToken);
            logger.CoursesSearched(page.Items.Count);
            return page.Map(course => CourseMapper.ToResponse(course));
        });
}
