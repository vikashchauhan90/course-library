using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Domain.Abstractions;
using CourseLibrary.Domain.Entities;
using CourseLibrary.Domain.Events;
using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Courses.Retire;

public sealed class RetireCourseCommandHandler(
    ICourseRepository repository,
    IRequestContext requestContext,
    IEventDispatcher eventDispatcher)
    : IHandler<RetireCourseCommand, CourseResponse?>
{
    public async Task<CourseResponse?> HandleAsync(RetireCourseCommand command, CancellationToken ct)
    {
        var current = await repository.GetByIdAsync(command.CourseId, ct);
        if (current is null)
            return null;

        if (!string.Equals(current.AuthorId, command.AuthorId, StringComparison.Ordinal))
            throw new UnauthorizedAccessException();

        if (current.RetiredAt is not null)
            return CourseMapper.ToResponse(current);

        var retiredAt = DateTimeOffset.UtcNow;
        var retired = current with { RetiredAt = retiredAt, UpdatedAt = retiredAt };
        await repository.UpsertAsync(retired, ct);
        await eventDispatcher.PublishAsync(
            new CourseRetiredEvent(
                retired.Id,
                Guid.NewGuid().ToString(),
                requestContext.UserId ?? "unknown",
                retiredAt,
                [
                    new() { Action = AuditAction.Updated, Name = nameof(retired.RetiredAt), Value = retired.RetiredAt },
                    new() { Action = AuditAction.Updated, Name = nameof(retired.UpdatedAt), Value = retired.UpdatedAt }
                ]),
            ct);

        return CourseMapper.ToResponse(retired);
    }
}