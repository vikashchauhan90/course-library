using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Core.Results;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.UseCases.Courses;
using CourseLibrary.Course.Application.UseCases.Courses;
using CourseLibrary.Course.Persistence.Repositories.Abstractions;

namespace CourseLibrary.Course.Application.UseCases.Courses.Search;

public interface ISearchCoursesUseCase
{
    Task<Result<PageResult<CourseResponse>>> ExecuteAsync(
        CourseSearchCriteria? criteria,
        int pageSize = 20,
        string? continuationToken = null,
        CancellationToken cancellationToken = default);
}

internal sealed class SearchCoursesUseCase(
    ICourseRepository repository,
    IValidator<SearchCoursesRequest> validator,
    ILogger<SearchCoursesUseCase> logger) : ISearchCoursesUseCase
{
    public Task<Result<PageResult<CourseResponse>>> ExecuteAsync(
        CourseSearchCriteria? criteria,
        int pageSize = 20,
        string? continuationToken = null,
        CancellationToken cancellationToken = default)
        => CourseApplicationUseCase.ExecuteResultAsync(logger, "courses.search", async () =>
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
