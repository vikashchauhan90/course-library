using CourseLibrary.Domain.Entities;

namespace CourseLibrary.Application.Abstractions.Repositories;

public interface ICommentRepository
{
    Task<Comment?> GetByIdAsync(string commentId, string courseId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Comment>> GetByCourseAsync(string courseId, CancellationToken cancellationToken = default);
    Task UpsertAsync(Comment comment, CancellationToken cancellationToken = default);
    Task DeleteAsync(string commentId, string courseId, CancellationToken cancellationToken = default);
}
