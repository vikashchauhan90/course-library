using CourseLibrary.Application.Operations.Courses.Update;
using CourseLibrary.Domain.Events;
using CourseLibrary.EventConsumer.Configuration.Observability.Metrics;
using CourseLibrary.EventConsumer.Configuration.Observability.Traces;
using MediatorForge.Abstractions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.EventConsumer.Consumers.Courses.UpdateCourse;

internal sealed class UpdateCourseAuditActivity(IDispatcher dispatcher, ILogger<UpdateCourseAuditActivity> logger)
{
    [Function(nameof(UpdateCourseAuditActivity))]
    public async Task RunAsync([ActivityTrigger] CourseUpdatedEvent courseEvent, CancellationToken cancellationToken)
    {
        var started = Stopwatch.GetTimestamp();
        using var activity = ActivitySources.EventConsumer.StartActivity("activity.update-course-audit", ActivityKind.Internal);
        try
        {
            await dispatcher.SendAsync<UpdateCourseAuditCommand, Unit>(new UpdateCourseAuditCommand(courseEvent.CourseId, courseEvent.AuthorId, courseEvent.Title, courseEvent.Description ?? string.Empty, courseEvent.AuthorName, courseEvent.ActorId, courseEvent.OccurredAt), cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
            Meters.ActivitiesCompleted.Add(1, new TagList { { "activity", nameof(UpdateCourseAuditActivity) } });
            Meters.ActivityDuration.Record(Stopwatch.GetElapsedTime(started).TotalMilliseconds, new TagList { { "activity", nameof(UpdateCourseAuditActivity) } });
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
            Meters.ActivitiesFailed.Add(1, new TagList { { "activity", nameof(UpdateCourseAuditActivity) } });
            logger.LogError(exception, "Failed to update course audit for CourseId {CourseId}.", courseEvent.CourseId);
            throw;
        }
    }
}
