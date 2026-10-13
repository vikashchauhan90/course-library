using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Core.Results;

namespace CourseLibrary.Course.Application.Operations;

public interface IDiscussionOperations
{
    Task<Result<DiscussionResponse>> GetAsync(
        string discussionId,
        string courseId,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<DiscussionResponse>>> GetByCourseAsync(
        string courseId,
        CancellationToken cancellationToken = default);

    Task<Result<DiscussionResponse>> CreateAsync(
        string courseId,
        string title,
        string description,
        CancellationToken cancellationToken = default);

    Task<Result<DiscussionResponse>> UpdateAsync(
        string discussionId,
        string courseId,
        string title,
        string description,
        CancellationToken cancellationToken = default);

    Task<Result<bool>> DeleteAsync(
        string discussionId,
        string courseId,
        CancellationToken cancellationToken = default);
}
