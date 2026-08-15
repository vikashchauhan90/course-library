using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;

namespace CourseLibrary.Application.Operations.Courses.Delete;

public sealed class DeleteCourseCommandHandler : IHandler<DeleteCourseCommand, bool>
{
    private readonly ICourseRepository _repository;
    private readonly IEventDispatcher _eventDispatcher;

    public DeleteCourseCommandHandler(ICourseRepository repository, IEventDispatcher eventDispatcher)
    {
        _repository = repository;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<bool> HandleAsync(DeleteCourseCommand command, CancellationToken ct)
    {
        await _repository.DeleteAsync(command.CourseId, command.PartitionKey, ct);
        await _eventDispatcher.PublishAsync(new CourseDeletedEvent(command.CourseId, command.PartitionKey), ct);
        return true;
    }
}
