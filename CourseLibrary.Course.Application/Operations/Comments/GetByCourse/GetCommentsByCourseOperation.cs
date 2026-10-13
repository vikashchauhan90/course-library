using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Persistence.Abstractions.Repositories;
using CourseLibrary.Course.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.Operations.Courses;

namespace CourseLibrary.Course.Application.Operations.Comments.GetByCourse;

public interface IGetCommentsByCourseOperation
{
    Task<Result<IReadOnlyList<CommentResponse>>> ExecuteAsync(string courseId, CancellationToken cancellationToken = default);
}

internal sealed class GetCommentsByCourseOperation(
    ICommentRepository repository,
    IValidator<GetCommentsByCourseRequest> validator,
    ILogger<GetCommentsByCourseOperation> logger) : IGetCommentsByCourseOperation
{
    public Task<Result<IReadOnlyList<CommentResponse>>> ExecuteAsync(string courseId, CancellationToken cancellationToken = default)
        => CourseApplicationOperation.ExecuteResultAsync(logger, "comments.get_by_course", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(new GetCommentsByCourseRequest(courseId), cancellationToken));
            var comments = await repository.GetByCourseAsync(OperationInput.CourseId(courseId), cancellationToken);
            return (IReadOnlyList<CommentResponse>)comments.Select(CourseMapper.ToResponse).ToArray();
        });
}
