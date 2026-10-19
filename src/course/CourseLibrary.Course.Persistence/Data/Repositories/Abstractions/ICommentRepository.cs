using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Domain.ValueObjects;

namespace CourseLibrary.Course.Persistence.Data.Repositories.Abstractions;

public interface ICommentRepository
{
    Task<Comment?> GetByIdAsync(CommentId commentId, CourseId courseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Comment>> GetByCourseAsync(CourseId courseId, CancellationToken cancellationToken = default);
    Task UpsertAsync(Comment comment, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(CommentId commentId, CourseId courseId, CancellationToken cancellationToken = default);
}
