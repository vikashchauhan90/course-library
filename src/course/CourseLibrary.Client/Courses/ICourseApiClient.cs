using CourseLibrary.Models;
using CourseLibrary.Models.Course;
using Hal.Core;

namespace CourseLibrary.Client.Courses;

public interface ICourseApiClient
{
    Task<IResource<CourseResponse>> GetCourseAsync(
        string courseId,
        CancellationToken cancellationToken = default);
    Task<IResource<PageResult<IResource<CourseResponse>>>> SearchAsync(
        CourseSearchCriteria? query,
        int pageSize = 20,
        string? continuationToken = null,
        CancellationToken cancellationToken = default);
    Task<IResource<PageResult<IResource<CourseResponse>>>> GetMineAsync(
        int pageSize = 20,
        string? continuationToken = null,
        CancellationToken cancellationToken = default);
    Task<IResource<CourseResponse>> CreateAsync(
        CreateCourseRequest request,
        CancellationToken cancellationToken = default);
    Task<IResource<CourseResponse>> UpdateAsync(
        string courseId,
        UpdateCourseRequest request,
        CancellationToken cancellationToken = default);
    Task<IResource<CourseResponse>> RetireAsync(
        string courseId,
        CancellationToken cancellationToken = default);
    Task DeleteAsync(
        string courseId,
        CancellationToken cancellationToken = default);
}
