using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Core.Results;

namespace CourseLibrary.Course.Application.Operations;

public interface ICommentOperations
{
    Task<Result<CommentResponse>> GetAsync(
        string commentId,
        string courseId,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<CommentResponse>>> GetByCourseAsync(
        string courseId,
        CancellationToken cancellationToken = default);

    Task<Result<CommentResponse>> CreateAsync(
        string courseId,
        string authorId,
        string content,
        string? parentCommentId = null,
        CancellationToken cancellationToken = default);

    Task<Result<CommentResponse>> UpdateAsync(
        string commentId,
        string courseId,
        string content,
        CancellationToken cancellationToken = default);

    Task<Result<bool>> DeleteAsync(
        string commentId,
        string courseId,
        CancellationToken cancellationToken = default);
}
