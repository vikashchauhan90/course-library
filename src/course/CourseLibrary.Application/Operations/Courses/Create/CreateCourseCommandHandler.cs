using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;
using CourseLibrary.Application.Abstractions.Repositories;

namespace CourseLibrary.Application.Operations.Courses.Create;

public sealed class CreateCourseCommandHandler : IHandler<CreateCourseCommand, Domain.Entities.Course>
{
    private readonly ICourseRepository _repository;
    private readonly ILogger<CreateCourseCommandHandler> _logger;

    public CreateCourseCommandHandler(ICourseRepository repository, ILogger<CreateCourseCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
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

        return course;
    }
}
