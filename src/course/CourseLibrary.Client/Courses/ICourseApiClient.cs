namespace CourseLibrary.Client.Courses;

public interface ICourseApiClient
{
    Task<CourseDetails?> GetCourseAsync(string courseId, string partitionKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CourseDetails>> SearchAsync(string? query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CourseDetails>> GetMineAsync(CancellationToken cancellationToken = default);
    Task<CourseDetails> CreateAsync(CreateCourseRequest request, CancellationToken cancellationToken = default);
    Task<CourseDetails> UpdateAsync(string courseId, string partitionKey, UpdateCourseRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(string courseId, string partitionKey, CancellationToken cancellationToken = default);
}
