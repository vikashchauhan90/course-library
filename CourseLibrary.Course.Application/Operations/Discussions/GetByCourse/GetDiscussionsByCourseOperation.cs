using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseLibrary.Course.Persistence.Abstractions.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.Operations.Courses;

namespace CourseLibrary.Course.Application.Operations.Discussions.GetByCourse;

public interface IGetDiscussionsByCourseOperation
{
    Task<Result<IReadOnlyList<DiscussionResponse>>> ExecuteAsync(string courseId, CancellationToken cancellationToken = default);
}

internal sealed class GetDiscussionsByCourseOperation(
    IDiscussionRepository repository,
    IValidator<GetDiscussionsByCourseRequest> validator,
    ILogger<GetDiscussionsByCourseOperation> logger) : IGetDiscussionsByCourseOperation
{
    public Task<Result<IReadOnlyList<DiscussionResponse>>> ExecuteAsync(string courseId, CancellationToken cancellationToken = default)
        => CourseApplicationOperation.ExecuteResultAsync(logger, "discussions.get_by_course", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(new GetDiscussionsByCourseRequest(courseId), cancellationToken));
            var discussions = await repository.GetByCourseAsync(OperationInput.CourseId(courseId), cancellationToken);
            return (IReadOnlyList<DiscussionResponse>)discussions.Select(CourseMapper.ToResponse).ToArray();
        });
}
