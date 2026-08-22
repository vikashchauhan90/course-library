using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Domain.Entities;
using CourseLibrary.Domain.Events;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Courses.Update;

public sealed class UpdateCourseCommandHandler(
    ICourseRepository repository,
    IRequestContext requestContext,
    ILogger<UpdateCourseCommandHandler> logger,
    IEventDispatcher eventDispatcher)
    : IHandler<UpdateCourseCommand, CourseResponse>
{
    public async Task<CourseResponse> HandleAsync(UpdateCourseCommand command, CancellationToken ct)
    {
        logger.UpdatingCourse(command.Id);
        var existing = await repository.GetByIdAsync(command.Id, command.AuthorId, ct);
        if (existing is null)
        {
            throw new KeyNotFoundException($"Course '{command.Id}' not found");
        }

        var updated = existing with
        {
            Title = command.Title,
            Description = command.Description,
            UpdatedAt = DateTime.UtcNow
        };

        await repository.UpsertAsync(updated, ct);
        await eventDispatcher.PublishAsync(
            new CourseUpdatedEvent(
                updated.Id,
                updated.AuthorId,
                updated.Title,
                updated.Description,
                Guid.NewGuid().ToString(),
                requestContext.UserId ?? "unknown",
                updated.UpdatedAt),
            ct);
        return CourseMapper.ToResponse(updated);
    }
}
