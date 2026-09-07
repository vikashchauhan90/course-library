using CourseLibrary.Models;
using CourseLibrary.Models.Course;

namespace CourseLibrary.Client.Courses;

public interface ICourseApiClient
{
    Task<CourseDetails?> GetCourseAsync(string courseId, CancellationToken cancellationToken = default);
    Task<PageResult<CourseDetails>> SearchAsync(
        string? query,
        int pageSize = 20,
        string? continuationToken = null,
        CancellationToken cancellationToken = default);
    Task<PageResult<CourseDetails>> GetMineAsync(
        int pageSize = 20,
        string? continuationToken = null,
        CancellationToken cancellationToken = default);
    Task<CourseDetails> CreateAsync(CreateCourseRequest request, CancellationToken cancellationToken = default);
    Task<CourseDetails> UpdateAsync(string courseId, UpdateCourseRequest request, CancellationToken cancellationToken = default);
    Task<CourseDetails> RetireAsync(string courseId, CancellationToken cancellationToken = default);
    Task DeleteAsync(string courseId, CancellationToken cancellationToken = default);
}
