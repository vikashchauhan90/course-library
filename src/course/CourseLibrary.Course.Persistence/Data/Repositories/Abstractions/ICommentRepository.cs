using CourseLibrary.Course.Domain.Entities;

namespace CourseLibrary.Course.Persistence.Data.Repositories.Abstractions;

public interface ICommentRepository
{
    Task<Comment?> GetByIdAsync(string commentId, string courseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Comment>> GetByCourseAsync(string courseId, CancellationToken cancellationToken = default);
    Task UpsertAsync(Comment comment, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string commentId, string courseId, CancellationToken cancellationToken = default);
}
