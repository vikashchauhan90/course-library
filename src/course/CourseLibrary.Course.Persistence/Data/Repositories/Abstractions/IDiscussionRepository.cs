using CourseLibrary.Course.Domain.Entities;

namespace CourseLibrary.Course.Persistence.Data.Repositories.Abstractions;

public interface IDiscussionRepository
{
    Task<Discussion?> GetByIdAsync(string discussionId, string courseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Discussion>> GetByCourseAsync(string courseId, CancellationToken cancellationToken = default);
    Task UpsertAsync(Discussion discussion, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string discussionId, string courseId, CancellationToken cancellationToken = default);
}
