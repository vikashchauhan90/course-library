using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;

namespace CourseLibrary.Application.Operations.Courses.Get;

public sealed class GetCourseQueryHandler : IHandler<GetCourseQuery, Domain.Entities.Course?>
{
    private readonly ICourseRepository _repository;

    public GetCourseQueryHandler(ICourseRepository repository)
    {
        _repository = repository;
    }

    public Task<Domain.Entities.Course?> HandleAsync(GetCourseQuery query, CancellationToken ct)
    {
        return _repository.GetByIdAsync(query.CourseId, query.PartitionKey, ct);
    }
}
