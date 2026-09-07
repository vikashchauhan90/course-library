using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Abstractions.RequestContext;
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
            AuthorName = command.AuthorName ?? existing.AuthorName,
            AuthorId = command.AuthorId,
            UpdatedAt = DateTime.UtcNow
        };

        await repository.UpsertAsync(updated, ct);
        await eventDispatcher.PublishAsync(
            new CourseUpdatedEvent(
                updated.Id,
                updated.AuthorId,
                updated.Title,
                updated.Description,
                updated.AuthorName,
                Guid.NewGuid().ToString(),
                requestContext.UserId ?? "unknown",
                updated.UpdatedAt,
                updated.CreatedAt,
                updated.UpdatedAt,
                updated.RetiredAt,
                updated.DeletedAt,
                GetChangedProperties(existing, updated)),
            ct);
        return CourseMapper.ToResponse(updated);
    }

    private static IReadOnlyList<string> GetChangedProperties(
        CourseLibrary.Domain.Entities.Course previous,
        CourseLibrary.Domain.Entities.Course current)
    {
        var changes = new List<string>();
        if (!string.Equals(previous.Title, current.Title, StringComparison.Ordinal)) changes.Add(nameof(current.Title));
        if (!string.Equals(previous.Description, current.Description, StringComparison.Ordinal)) changes.Add(nameof(current.Description));
        if (!string.Equals(previous.AuthorName, current.AuthorName, StringComparison.Ordinal)) changes.Add(nameof(current.AuthorName));
        if (previous.RetiredAt != current.RetiredAt) changes.Add(nameof(current.RetiredAt));
        if (previous.DeletedAt != current.DeletedAt) changes.Add(nameof(current.DeletedAt));
        if (changes.Count == 0) changes.Add(nameof(current.UpdatedAt));
        return changes;
    }
}
