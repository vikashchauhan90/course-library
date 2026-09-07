using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Domain.Events;
using CourseLibrary.Domain.Abstractions;
using CourseLibrary.Domain.Entities;
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
                Guid.NewGuid().ToString(),
                requestContext.UserId ?? "unknown",
                updated.UpdatedAt,
                GetChangedProperties(existing, updated)),
            ct);
        return CourseMapper.ToResponse(updated);
    }

    private static IReadOnlyList<AuditEntry> GetChangedProperties(
        Course previous,
        Course current)
    {
        var changes = new List<AuditEntry>();
        AddIfChanged(changes, nameof(current.Title), previous.Title, current.Title);
        AddIfChanged(changes, nameof(current.Description), previous.Description, current.Description);
        AddIfChanged(changes, nameof(current.AuthorName), previous.AuthorName, current.AuthorName);
        AddIfChanged(changes, nameof(current.RetiredAt), previous.RetiredAt, current.RetiredAt);
        AddIfChanged(changes, nameof(current.DeletedAt), previous.DeletedAt, current.DeletedAt);
        if (changes.Count == 0)
            changes.Add(new AuditEntry { Action = AuditAction.Updated, Name = nameof(current.UpdatedAt), Value = current.UpdatedAt });
        return changes;
    }

    private static void AddIfChanged<T>(ICollection<AuditEntry> changes, string name, T previous, T current)
    {
        if (!EqualityComparer<T>.Default.Equals(previous, current))
            changes.Add(new AuditEntry { Action = AuditAction.Updated, Name = name, Value = current });
    }
}
