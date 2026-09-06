using CourseLibrary.Application.Operations.Courses.Delete;
using CourseLibrary.Domain.Events;
using CourseLibrary.EventConsumer.Configuration.Observability.Metrics;
using CourseLibrary.EventConsumer.Configuration.Observability.Traces;
using MediatorForge.Abstractions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.EventConsumer.Consumers.Courses.DeleteCourse;

internal sealed class DeleteCourseAuditActivity(IDispatcher dispatcher, ILogger<DeleteCourseAuditActivity> logger)
{
    [Function(nameof(DeleteCourseAuditActivity))]
    public async Task RunAsync([ActivityTrigger] CourseDeletedEvent courseEvent, CancellationToken cancellationToken)
    {
        var started = Stopwatch.GetTimestamp();
        using var activity = ActivitySources.EventConsumer.StartActivity("activity.delete-course-audit", ActivityKind.Internal);
        try
        {
            await dispatcher.SendAsync<DeleteCourseAuditCommand, Unit>(new DeleteCourseAuditCommand(courseEvent.CourseId, courseEvent.AuthorId, courseEvent.Title, courseEvent.Description, courseEvent.ActorId, courseEvent.OccurredAt), cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
            Meters.ActivitiesCompleted.Add(1, new TagList { { "activity", nameof(DeleteCourseAuditActivity) } });
            Meters.ActivityDuration.Record(Stopwatch.GetElapsedTime(started).TotalMilliseconds, new TagList { { "activity", nameof(DeleteCourseAuditActivity) } });
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
            Meters.ActivitiesFailed.Add(1, new TagList { { "activity", nameof(DeleteCourseAuditActivity) } });
            logger.LogError(exception, "Failed to delete course audit for CourseId {CourseId}.", courseEvent.CourseId);
            throw;
        }
    }
}
