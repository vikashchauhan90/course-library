using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Course.Contracts.Pagination;
using CourseLibrary.Core.Results;

namespace CourseLibrary.Course.Application.Operations;

public interface ICourseOperations
{
    Task<Result<CourseResponse>> GetAsync(string courseId, CancellationToken cancellationToken = default);

    Task<Result<PageResult<CourseResponse>>> SearchAsync(
        CourseSearchCriteria? criteria,
        int pageSize = 20,
        string? continuationToken = null,
        CancellationToken cancellationToken = default);

    Task<Result<CourseResponse>> CreateAsync(
        CreateCourseRequest request,
        string actorId,
        CancellationToken cancellationToken = default);

    Task<Result<CourseResponse>> UpdateAsync(
        string courseId,
        UpdateCourseRequest request,
        string actorId,
        CancellationToken cancellationToken = default);

    Task<Result<CourseResponse>> RetireAsync(
        string courseId,
        string actorId,
        CancellationToken cancellationToken = default);

    Task<Result<bool>> DeleteAsync(
        string courseId,
        string actorId,
        CancellationToken cancellationToken = default);
}
