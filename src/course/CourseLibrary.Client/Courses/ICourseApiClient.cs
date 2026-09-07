using CourseLibrary.Models;
using CourseLibrary.Models.Course;
using Hal.Core;

namespace CourseLibrary.Client.Courses;

public interface ICourseApiClient
{
    Task<IResource<CourseDetails>> GetCourseAsync(
        string courseId,
        CancellationToken cancellationToken = default);
    Task<IResource<PageResult<IResource<CourseDetails>>>> SearchAsync(
        string? query,
        int pageSize = 20,
        string? continuationToken = null,
        CancellationToken cancellationToken = default);
    Task<IResource<PageResult<IResource<CourseDetails>>>> GetMineAsync(
        int pageSize = 20,
        string? continuationToken = null,
        CancellationToken cancellationToken = default);
    Task<IResource<CourseDetails>> CreateAsync(
        CreateCourseRequest request,
        CancellationToken cancellationToken = default);
    Task<IResource<CourseDetails>> UpdateAsync(
        string courseId,
        UpdateCourseRequest request,
        CancellationToken cancellationToken = default);
    Task<IResource<CourseDetails>> RetireAsync(
        string courseId,
        CancellationToken cancellationToken = default);
    Task DeleteAsync(
        string courseId,
        CancellationToken cancellationToken = default);
}
