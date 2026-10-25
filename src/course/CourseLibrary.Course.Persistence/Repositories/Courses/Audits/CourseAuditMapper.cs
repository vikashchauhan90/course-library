using CourseLibrary.Course.Contracts.DTOs.Courses.Audits;
using CourseLibrary.Course.Persistence.Entities;

namespace CourseLibrary.Course.Persistence.Repositories.Courses.Audits;

internal static class CourseAuditMapper
{
    public static CourseAuditResponse ToResponse(
        this CourseAuditEntry entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new CourseAuditResponse
        {
            Id = entity.Id,
            CourseId = entity.CourseId,
            ActorId = entity.ActorId,
            Action = entity.Action,
            EventId = entity.EventId,
            OccurredAt = entity.OccurredAt,
            ChangedProperties = entity.ChangedProperties
        };
    }

    public static CourseAuditEntry ToEntity(
        this CourseAuditRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.EventId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.CourseId);

        var partitionKeyValue =
            CoursePartitionKeyFormats
            .ForCourse(request.CourseId);

        return  new CourseAuditEntry
        {
            Id = request.EventId,
            CourseId = request.CourseId,
            PartitionKey = partitionKeyValue,
            ActorId = request.ActorId,
            Action = request.Action,
            EventId = request.EventId,
            OccurredAt = request.OccurredAt,
            ChangedProperties = request.ChangedProperties,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
