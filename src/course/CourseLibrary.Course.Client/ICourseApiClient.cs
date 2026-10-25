using CourseLibrary.Platform.Results;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Course.Contracts.DTOs.Courses;

namespace CourseLibrary.Course.Client;

public interface ICourseApiClient
{
    Task<CourseResponse> GetCourseAsync(
        string courseId,
        CancellationToken cancellationToken = default);
    Task<PageResult<CourseResponse>> SearchAsync(
        CourseSearchCriteria? query,
        int pageSize = 20,
        string? continuationToken = null,
        CancellationToken cancellationToken = default);
    Task<CourseResponse> CreateAsync(
        CourseRequest request,
        CancellationToken cancellationToken = default);
    Task<CourseResponse> UpdateAsync(
        string courseId,
        UpdateCourseRequest request,
        CancellationToken cancellationToken = default);
    Task<CourseResponse> RetireAsync(
        string courseId,
        CancellationToken cancellationToken = default);
    Task DeleteAsync(
        string courseId,
        CancellationToken cancellationToken = default);
}
