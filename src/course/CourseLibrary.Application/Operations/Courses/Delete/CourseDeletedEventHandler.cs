using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Courses.Delete;

public sealed class CourseDeletedEventHandler : IEventNotificationHandler<CourseDeletedEvent>
{
    private readonly ILogger<CourseDeletedEventHandler> _logger;

    public CourseDeletedEventHandler(ILogger<CourseDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(IEventNotification<CourseDeletedEvent> notification, CancellationToken ct)
    {
        var ev = notification.Event;
        _logger.LogInformation("Course deleted: {CourseId} partition {PartitionKey}", ev.CourseId, ev.PartitionKey);
        return Task.CompletedTask;
    }
}
