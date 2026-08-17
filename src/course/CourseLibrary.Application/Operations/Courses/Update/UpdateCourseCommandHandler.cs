using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Operations.Courses;

namespace CourseLibrary.Application.Operations.Courses.Update;

public sealed class UpdateCourseCommandHandler : IHandler<UpdateCourseCommand, CourseResponse>
{
    private readonly ICourseRepository _repository;
    private readonly IEventDispatcher _eventDispatcher;

    public UpdateCourseCommandHandler(ICourseRepository repository, IEventDispatcher eventDispatcher)
    {
        _repository = repository;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<CourseResponse> HandleAsync(UpdateCourseCommand command, CancellationToken ct)
    {
        var existing = await _repository.GetByIdAsync(command.Id, command.AuthorId, ct);
        if (existing is null)
        {
            throw new KeyNotFoundException($"Course '{command.Id}' not found");
        }

        var updated = existing with
        {
            Title = command.Title,
            Description = command.Description,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.UpsertAsync(updated, ct);
        await _eventDispatcher.PublishAsync(new CourseUpdatedEvent(updated.Id, updated.AuthorId, updated.Title, updated.UpdatedAt), ct);
        return CourseMapper.ToResponse(updated);
    }
}
