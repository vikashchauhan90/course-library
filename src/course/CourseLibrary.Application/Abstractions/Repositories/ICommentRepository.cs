using CourseLibrary.Domain.Entities;
using CourseLibrary.Domain.ValueObjects;

namespace CourseLibrary.Application.Abstractions.Repositories;

public interface ICommentRepository
{
    Task<Comment?> GetByIdAsync(CommentId commentId, CourseId courseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Comment>> GetByCourseAsync(CourseId courseId, CancellationToken cancellationToken = default);
    Task UpsertAsync(Comment comment, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(CommentId commentId, CourseId courseId, CancellationToken cancellationToken = default);
}
