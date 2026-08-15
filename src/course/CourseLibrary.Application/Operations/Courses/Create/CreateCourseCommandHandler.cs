using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;
using CourseLibrary.Application.Abstractions.Repositories;

namespace CourseLibrary.Application.Operations.Courses.Create;

public sealed class CreateCourseCommandHandler : IHandler<CreateCourseCommand, Domain.Entities.Course>
{
    private readonly ICourseRepository _repository;
    private readonly ILogger<CreateCourseCommandHandler> _logger;
    private readonly IEventDispatcher _eventDispatcher;

    public CreateCourseCommandHandler(ICourseRepository repository, ILogger<CreateCourseCommandHandler> logger, IEventDispatcher eventDispatcher)
    {
        _repository = repository;
        _logger = logger;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<Domain.Entities.Course> HandleAsync(CreateCourseCommand command, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var course = new Domain.Entities.Course
        {
            Id = Guid.NewGuid().ToString(),
            Title = command.Title,
            Description = command.Description,
            AuthorId = command.AuthorId,
            CreatedAt = now,
            UpdatedAt = now
        };

        _logger.LogInformation("Creating course {CourseId} for author {AuthorId}", course.Id, course.AuthorId);

        await _repository.UpsertAsync(course, ct);

        // Publish course created event for downstream consumers
        await _eventDispatcher.PublishAsync(new CourseCreatedEvent(course.Id, course.AuthorId, course.Title, course.CreatedAt), ct);

        return course;
    }
}
