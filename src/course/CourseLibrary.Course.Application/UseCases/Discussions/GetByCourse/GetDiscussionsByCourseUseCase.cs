using CourseLibrary.Course.Application.Core;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.UseCases.Courses;
using CourseLibrary.Course.Contracts.DTOs.Courses.Discussions;
using CourseLibrary.Course.Persistence.Repositories.Courses.Discussions;

namespace CourseLibrary.Course.Application.UseCases.Discussions.GetByCourse;

public interface IGetDiscussionsByCourseUseCase
{
    Task<Result<IReadOnlyList<DiscussionResponse>>> ExecuteAsync(string courseId, CancellationToken cancellationToken = default);
}

internal sealed class GetDiscussionsByCourseUseCase(
    IDiscussionRepository repository,
    IValidator<GetDiscussionsByCourseRequest> validator,
    ILogger<GetDiscussionsByCourseUseCase> logger) : IGetDiscussionsByCourseUseCase
{
    public Task<Result<IReadOnlyList<DiscussionResponse>>> ExecuteAsync(string courseId, CancellationToken cancellationToken = default)
        => CourseApplicationUseCase.ExecuteResultAsync(logger, "discussions.get_by_course", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(new GetDiscussionsByCourseRequest(courseId), cancellationToken));
            var discussions = await repository.GetByCourseAsync(UseCaseInput.CourseId(courseId), cancellationToken);
            return (IReadOnlyList<DiscussionResponse>)discussions.Select(CourseMapper.ToResponse).ToArray();
        });
}
