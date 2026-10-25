using CourseLibrary.Course.Application.Core;
using CourseLibrary.Core.Domain.Exceptions;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.UseCases.Courses;
using CourseLibrary.Course.Contracts.DTOs.Courses;
using CourseLibrary.Course.Persistence.Repositories.Courses.Discussions;
using CourseLibrary.Course.Persistence.Repositories.Courses;
using CourseLibrary.Course.Persistence.Repositories.Courses.Comments;

namespace CourseLibrary.Course.Application.UseCases.Courses.Get;

public interface IGetCourseUseCase
{
    Task<Result<CourseResponse>> ExecuteAsync(string courseId, CancellationToken cancellationToken = default);
}

internal sealed class GetCourseUseCase(
    ICourseRepository courseRepository,
    ICommentRepository commentRepository,
    IDiscussionRepository discussionRepository,
    IValidator<GetCourseRequest> validator,
    ILogger<GetCourseUseCase> logger) : IGetCourseUseCase
{
    public Task<Result<CourseResponse>> ExecuteAsync(string courseId, CancellationToken cancellationToken = default)
        => CourseApplicationUseCase.ExecuteResultAsync(logger, "courses.get", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(new GetCourseRequest(courseId), cancellationToken));
            var id = UseCaseInput.CourseId(courseId);
            var course = await courseRepository.GetByIdAsync(id, cancellationToken)
                ?? throw new DataNotFoundException($"Course '{courseId}' was not found.");

            var commentsTask = commentRepository.GetByCourseAsync(id, cancellationToken);
            var discussionsTask = discussionRepository.GetByCourseAsync(id, cancellationToken);
            await Task.WhenAll(commentsTask, discussionsTask);

            return CourseMapper.ToResponse(
                course,
                (await commentsTask).Select(CourseMapper.ToResponse).ToArray(),
                (await discussionsTask).Select(CourseMapper.ToResponse).ToArray());
        });
}
