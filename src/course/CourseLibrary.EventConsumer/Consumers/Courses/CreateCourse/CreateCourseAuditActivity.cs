using CourseLibrary.Application.Operations.Courses.Create;
using CourseLibrary.Domain.Events;
using CourseLibrary.EventConsumer.Configuration.Observability.Metrics;
using CourseLibrary.EventConsumer.Configuration.Observability.Traces;
using MediatorForge.Abstractions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.EventConsumer.Consumers.Courses.CreateCourse;

internal sealed class CreateCourseAuditActivity(IDispatcher dispatcher, ILogger<CreateCourseAuditActivity> logger)
{
    [Function(nameof(CreateCourseAuditActivity))]
    public async Task RunAsync([ActivityTrigger] CourseCreatedEvent courseEvent, CancellationToken cancellationToken)
    {
        var started = Stopwatch.GetTimestamp();
        using var activity = ActivitySources.EventConsumer.StartActivity("activity.create-course-audit", ActivityKind.Internal);
        try
        {
            await dispatcher.SendAsync<CreateCourseAuditCommand, Unit>(new CreateCourseAuditCommand(courseEvent.CourseId, courseEvent.AuthorId, courseEvent.Title, courseEvent.Description, courseEvent.AuthorName, courseEvent.EventId, courseEvent.ActorId, courseEvent.OccurredAt, courseEvent.CreatedAt, courseEvent.UpdatedAt, courseEvent.RetiredAt, courseEvent.DeletedAt, courseEvent.ChangedProperties), cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
            Meters.ActivitiesCompleted.Add(1, new TagList { { "activity", nameof(CreateCourseAuditActivity) } });
            Meters.ActivityDuration.Record(Stopwatch.GetElapsedTime(started).TotalMilliseconds, new TagList { { "activity", nameof(CreateCourseAuditActivity) } });
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
            Meters.ActivitiesFailed.Add(1, new TagList { { "activity", nameof(CreateCourseAuditActivity) } });
            logger.LogError(exception, "Failed to create course audit for CourseId {CourseId}.", courseEvent.CourseId);
            throw;
        }
    }
}
