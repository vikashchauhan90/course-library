using CourseLibrary.Course.Application.Observability;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Course.Contracts.Pagination;
using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Domain.Events;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseLibrary.Course.Persistence.Abstractions.Repositories;
using CourseLibrary.Platform.Messaging.Abstractions;
using CourseLibrary.Platform.Serialization.Abstractions;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Diagnostics;
using CourseEntity = CourseLibrary.Course.Domain.Entities.Course;

namespace CourseLibrary.Course.Application.Operations;

public sealed class CourseOperations(
    ICourseRepository courseRepository,
    ICommentRepository commentRepository,
    IDiscussionRepository discussionRepository,
    IEventPublisher eventPublisher,
    ISerializerFactory serializerFactory,
    ILogger<CourseOperations> logger)
{
    public Task<CourseResponse?> GetAsync(
        string courseId,
        CancellationToken cancellationToken = default)
    {
        var id = OperationInput.CourseId(courseId);
        return CourseApplicationOperation.ExecuteAsync(
            logger,
            "courses.get",
            async () =>
            {
                var course = await courseRepository.GetByIdAsync(id, cancellationToken);
                if (course is null)
                {
                    return null;
                }

                var commentsTask = commentRepository.GetByCourseAsync(id, cancellationToken);
                var discussionsTask = discussionRepository.GetByCourseAsync(id, cancellationToken);
                await Task.WhenAll(commentsTask, discussionsTask);

                return CourseMapper.ToResponse(
                    course,
                    (await commentsTask).Select(CourseMapper.ToResponse).ToArray(),
                    (await discussionsTask).Select(CourseMapper.ToResponse).ToArray());
            });
    }

    public Task<PageResult<CourseResponse>> SearchAsync(
        CourseSearchCriteria? criteria,
        int pageSize = 20,
        string? continuationToken = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);
        var searchCriteria = criteria ?? new CourseSearchCriteria(null, null, false, false);

        return CourseApplicationOperation.ExecuteAsync(
            logger,
            "courses.search",
            async () =>
            {
                var page = await courseRepository.SearchAsync(
                    searchCriteria,
                    pageSize,
                    continuationToken,
                    cancellationToken);
                return page.Map(course => CourseMapper.ToResponse(course));
            });
    }

    public Task<CourseResponse> CreateAsync(
        CreateCourseRequest request,
        string actorId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        OperationInput.Required(request.Title, nameof(request.Title));
        OperationInput.Required(request.Description, nameof(request.Description));
        OperationInput.Required(request.AuthorId, nameof(request.AuthorId));
        OperationInput.Required(request.AuthorName, nameof(request.AuthorName));
        OperationInput.Required(actorId, nameof(actorId));

        return CourseApplicationOperation.ExecuteAsync(
            logger,
            "courses.create",
            async () =>
            {
                var now = DateTimeOffset.UtcNow;
                var course = new CourseEntity
                {
                    Id = CourseId.New(),
                    Title = request.Title,
                    Description = request.Description,
                    AuthorId = request.AuthorId,
                    AuthorName = request.AuthorName,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                await courseRepository.UpsertAsync(course, cancellationToken);
                await PublishEventAsync(
                    course,
                    actorId,
                    CourseEventType.Created,
                    now,
                    [
                        Added(nameof(course.Title), course.Title),
                        Added(nameof(course.Description), course.Description),
                        Added(nameof(course.AuthorId), course.AuthorId.Value),
                        Added(nameof(course.AuthorName), course.AuthorName),
                        Added(nameof(course.CreatedAt), course.CreatedAt),
                        Added(nameof(course.UpdatedAt), course.UpdatedAt)
                    ],
                    cancellationToken);

                return CourseMapper.ToResponse(course);
            });
    }

    public Task<CourseResponse?> UpdateAsync(
        string courseId,
        UpdateCourseRequest request,
        string actorId,
        CancellationToken cancellationToken = default)
    {
        var id = OperationInput.CourseId(courseId);
        ArgumentNullException.ThrowIfNull(request);
        OperationInput.Required(request.Title, nameof(request.Title));
        OperationInput.Required(request.Description, nameof(request.Description));
        OperationInput.Required(actorId, nameof(actorId));

        return CourseApplicationOperation.ExecuteAsync(
            logger,
            "courses.update",
            async () =>
            {
                var course = await courseRepository.GetByIdAsync(id, cancellationToken);
                if (course is null)
                {
                    return null;
                }

                EnsureAuthor(course, actorId);
                var now = DateTimeOffset.UtcNow;
                var updated = course with
                {
                    Title = request.Title,
                    Description = request.Description,
                    UpdatedAt = now
                };

                await courseRepository.UpsertAsync(updated, cancellationToken);
                var changes = new List<AuditEntry>();
                AddIfChanged(changes, nameof(updated.Title), course.Title, updated.Title);
                AddIfChanged(changes, nameof(updated.Description), course.Description, updated.Description);
                if (changes.Count == 0)
                {
                    changes.Add(new AuditEntry
                    {
                        Action = AuditAction.Updated,
                        Name = nameof(updated.UpdatedAt),
                        Value = updated.UpdatedAt
                    });
                }

                await PublishEventAsync(
                    updated,
                    actorId,
                    CourseEventType.Updated,
                    now,
                    changes,
                    cancellationToken);

                return CourseMapper.ToResponse(updated);
            });
    }

    public Task<CourseResponse?> RetireAsync(
        string courseId,
        string actorId,
        CancellationToken cancellationToken = default)
    {
        var id = OperationInput.CourseId(courseId);
        OperationInput.Required(actorId, nameof(actorId));

        return CourseApplicationOperation.ExecuteAsync(
            logger,
            "courses.retire",
            async () =>
            {
                var course = await courseRepository.GetByIdAsync(id, cancellationToken);
                if (course is null)
                {
                    return null;
                }

                EnsureAuthor(course, actorId);
                if (course.RetiredAt is not null)
                {
                    return CourseMapper.ToResponse(course);
                }

                var now = DateTimeOffset.UtcNow;
                var retired = course with { RetiredAt = now, UpdatedAt = now };
                await courseRepository.UpsertAsync(retired, cancellationToken);
                await PublishEventAsync(
                    retired,
                    actorId,
                    CourseEventType.Retired,
                    now,
                    [
                        Added(nameof(retired.RetiredAt), retired.RetiredAt),
                        Updated(nameof(retired.UpdatedAt), retired.UpdatedAt)
                    ],
                    cancellationToken);

                return CourseMapper.ToResponse(retired);
            });
    }

    public Task<bool> DeleteAsync(
        string courseId,
        string actorId,
        CancellationToken cancellationToken = default)
    {
        var id = OperationInput.CourseId(courseId);
        OperationInput.Required(actorId, nameof(actorId));

        return CourseApplicationOperation.ExecuteAsync(
            logger,
            "courses.delete",
            async () =>
            {
                var course = await courseRepository.GetByIdAsync(id, cancellationToken);
                if (course is null)
                {
                    return false;
                }

                EnsureAuthor(course, actorId);
                if (course.DeletedAt is not null)
                {
                    return false;
                }

                var now = DateTimeOffset.UtcNow;
                var deleted = course with { DeletedAt = now, UpdatedAt = now };
                await courseRepository.UpsertAsync(deleted, cancellationToken);
                await PublishEventAsync(
                    deleted,
                    actorId,
                    CourseEventType.Deleted,
                    now,
                    [
                        Added(nameof(deleted.DeletedAt), deleted.DeletedAt),
                        Updated(nameof(deleted.UpdatedAt), deleted.UpdatedAt)
                    ],
                    cancellationToken);

                return true;
            });
    }

    private async Task PublishEventAsync(
        CourseEntity course,
        string actorId,
        CourseEventType eventKind,
        DateTimeOffset occurredAt,
        IReadOnlyList<AuditEntry> changes,
        CancellationToken cancellationToken)
    {
        var eventId = Guid.NewGuid();
        var courseEvent = new CourseEvent
        {
            EventId = eventId.ToString(),
            CourseId = course.Id,
            ActorId = actorId,
            OccurredAt = occurredAt,
            EventKind = eventKind,
            ChangedProperties = changes
        };

        var payload = serializerFactory
            .Create<CourseEvent>(SerializerType.Json)
            .Serialize(courseEvent);
        var currentActivity = Activity.Current;

        await eventPublisher.PublishAsync(
            nameof(CourseEvent),
            new ServiceBusEventPayload
            {
                EventId = eventId,
                EventType = nameof(CourseEvent),
                OccurredAt = occurredAt,
                Payload = new BinaryData(payload),
                Source = "CourseLibrary.Course.Application",
                Subject = course.Id.ToString(),
                Metadata = new EventMetadata
                {
                    TraceParent = currentActivity?.Id,
                    TraceState = currentActivity?.TraceStateString,
                    Properties = ImmutableDictionary<string, string>.Empty
                        .Add("actorId", actorId)
                        .Add("courseId", course.Id.ToString())
                }
            },
            cancellationToken);
    }

    private static void EnsureAuthor(CourseEntity course, string actorId)
    {
        if (!string.Equals(course.AuthorId.Value, actorId, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("The current user is not authorized to modify this course.");
        }
    }

    private static AuditEntry Added(string name, object? value)
        => new()
        {
            Action = AuditAction.Add,
            Name = name,
            Value = value,
            ValueTypeName = value?.GetType().Name
        };

    private static AuditEntry Updated(string name, object? value)
        => new()
        {
            Action = AuditAction.Updated,
            Name = name,
            Value = value,
            ValueTypeName = value?.GetType().Name
        };

    private static void AddIfChanged<T>(
        ICollection<AuditEntry> changes,
        string name,
        T previous,
        T current)
    {
        if (!EqualityComparer<T>.Default.Equals(previous, current))
        {
            changes.Add(Updated(name, current));
        }
    }
}
