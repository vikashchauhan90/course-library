using CourseLibrary.Course.Contracts.DTOs.Courses.Comments;

namespace CourseLibrary.Course.Persistence.Repositories.Courses.Comments;

public interface ICommentRepository
{
    Task<CommentResponse?> GetByIdAsync(
        string commentId,
        string courseId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CommentResponse>> GetByCourseAsync(
        string courseId,
        CancellationToken cancellationToken = default);
    Task UpsertAsync(
        CommentRequest comment,
        CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(
        string commentId,
        string courseId,
        CancellationToken cancellationToken = default);
}
