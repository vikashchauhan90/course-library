using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Application.Operations.Authors;
using CourseLibrary.Domain.Entities;
using CourseLibrary.Domain.Events;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Courses.Create;

public sealed class CreateCourseCommandHandler(
    ICourseRepository repository,
    IAuthorRepository authorRepository,
    IRequestContext requestContext,
    ILogger<CreateCourseCommandHandler> logger,
    IEventDispatcher eventDispatcher) 
    : IHandler<CreateCourseCommand, CourseResponse>
{
    public async Task<CourseResponse> HandleAsync(CreateCourseCommand command, CancellationToken ct)
    {
        var author = await authorRepository.GetByIdAsync(command.AuthorId, ct);
        if (author is null)
            throw new KeyNotFoundException($"Author '{command.AuthorId}' not found");

        var now = DateTime.UtcNow;
        var course = new Course
        {
            Id = Guid.NewGuid().ToString(),
            Title = command.Title,
            Description = command.Description,
            AuthorId = command.AuthorId,
            CreatedAt = now,
            UpdatedAt = now
        };

        logger.PersistingCourse(course.Id, course.AuthorId);

        await repository.UpsertAsync(course, ct);

        // Publish course created event for downstream consumers
        await eventDispatcher.PublishAsync(
            new CourseCreatedEvent(
                course.Id,
                course.AuthorId,
                course.Title,
                course.Description,
                Guid.NewGuid().ToString(),
                requestContext.UserId ?? "unknown",
                course.CreatedAt),
            ct);

        return CourseMapper.ToResponse(course, AuthorMapper.ToResponse(author));
    }
}
