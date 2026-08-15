using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;

namespace CourseLibrary.Application.Operations.Courses.Update;

public sealed class UpdateCourseCommandHandler : IHandler<UpdateCourseCommand, Domain.Entities.Course>
{
    private readonly ICourseRepository _repository;

    public UpdateCourseCommandHandler(ICourseRepository repository)
    {
        _repository = repository;
    }

    public async Task<Domain.Entities.Course> HandleAsync(UpdateCourseCommand command, CancellationToken ct)
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
        return updated;
    }
}
