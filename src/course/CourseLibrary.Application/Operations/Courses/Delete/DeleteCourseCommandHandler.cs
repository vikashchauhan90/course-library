using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;

namespace CourseLibrary.Application.Operations.Courses.Delete;

public sealed class DeleteCourseCommandHandler : IHandler<DeleteCourseCommand, bool>
{
    private readonly ICourseRepository _repository;

    public DeleteCourseCommandHandler(ICourseRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> HandleAsync(DeleteCourseCommand command, CancellationToken ct)
    {
        await _repository.DeleteAsync(command.CourseId, command.PartitionKey, ct);
        return true;
    }
}
