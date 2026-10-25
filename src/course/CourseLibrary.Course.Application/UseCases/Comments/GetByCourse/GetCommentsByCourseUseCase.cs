using CourseLibrary.Course.Application.Core;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.UseCases.Courses;
using CourseLibrary.Course.Contracts.DTOs.Courses.Comments;
using CourseLibrary.Course.Persistence.Repositories.Courses.Comments;

namespace CourseLibrary.Course.Application.UseCases.Comments.GetByCourse;

public interface IGetCommentsByCourseUseCase
{
    Task<Result<IReadOnlyList<CommentResponse>>> ExecuteAsync(string courseId, CancellationToken cancellationToken = default);
}

internal sealed class GetCommentsByCourseUseCase(
    ICommentRepository repository,
    IValidator<GetCommentsByCourseRequest> validator,
    ILogger<GetCommentsByCourseUseCase> logger) : IGetCommentsByCourseUseCase
{
    public Task<Result<IReadOnlyList<CommentResponse>>> ExecuteAsync(string courseId, CancellationToken cancellationToken = default)
        => CourseApplicationUseCase.ExecuteResultAsync(logger, "comments.get_by_course", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(new GetCommentsByCourseRequest(courseId), cancellationToken));
            var comments = await repository.GetByCourseAsync(UseCaseInput.CourseId(courseId), cancellationToken);
            return (IReadOnlyList<CommentResponse>)comments.Select(CourseMapper.ToResponse).ToArray();
        });
}
