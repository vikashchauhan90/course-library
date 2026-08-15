using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Courses.Update;

public sealed class CourseUpdatedEventHandler : IEventNotificationHandler<CourseUpdatedEvent>
{
    private readonly ILogger<CourseUpdatedEventHandler> _logger;

    public CourseUpdatedEventHandler(ILogger<CourseUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(IEventNotification<CourseUpdatedEvent> notification, CancellationToken ct)
    {
        var ev = notification.Event;
        _logger.LogInformation("Course updated: {CourseId} by {AuthorId} at {UpdatedAt}", ev.CourseId, ev.AuthorId, ev.UpdatedAt);
        return Task.CompletedTask;
    }
}
