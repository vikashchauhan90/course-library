using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Core.Domain.Exceptions;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseLibrary.Course.Persistence.Abstractions.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Course.Application.Operations.Courses.Get;

public interface IGetCourseOperation
{
    Task<Result<CourseResponse>> ExecuteAsync(string courseId, CancellationToken cancellationToken = default);
}

internal sealed class GetCourseOperation(
    ICourseRepository courseRepository,
    ICommentRepository commentRepository,
    IDiscussionRepository discussionRepository,
    IValidator<GetCourseRequest> validator,
    ILogger<GetCourseOperation> logger) : IGetCourseOperation
{
    public Task<Result<CourseResponse>> ExecuteAsync(string courseId, CancellationToken cancellationToken = default)
        => CourseApplicationOperation.ExecuteResultAsync(logger, "courses.get", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(new GetCourseRequest(courseId), cancellationToken));
            var id = OperationInput.CourseId(courseId);
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
