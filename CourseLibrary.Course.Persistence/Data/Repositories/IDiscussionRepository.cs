using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Domain.ValueObjects;

namespace CourseLibrary.Course.Persistence.Data.Repositories;

public interface IDiscussionRepository
{
    Task<Discussion?> GetByIdAsync(DiscussionId discussionId, CourseId courseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Discussion>> GetByCourseAsync(CourseId courseId, CancellationToken cancellationToken = default);
    Task UpsertAsync(Discussion discussion, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(DiscussionId discussionId, CourseId courseId, CancellationToken cancellationToken = default);
}
