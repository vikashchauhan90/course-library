using CourseLibrary.Course.Contracts.DTOs.Courses.Discussions;

namespace CourseLibrary.Course.Persistence.Repositories.Courses.Discussions;

public interface IDiscussionRepository
{
    Task<DiscussionResponse?> GetByIdAsync(string discussionId, string courseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DiscussionResponse>> GetByCourseAsync(string courseId, CancellationToken cancellationToken = default);
    Task UpsertAsync(DiscussionResponse discussion, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string discussionId, string courseId, CancellationToken cancellationToken = default);
}
