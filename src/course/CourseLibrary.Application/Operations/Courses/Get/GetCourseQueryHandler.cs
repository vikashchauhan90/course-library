using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Operations.Courses;

namespace CourseLibrary.Application.Operations.Courses.Get;

public sealed class GetCourseQueryHandler : IHandler<GetCourseQuery, CourseResponse?>
{
    private readonly ICourseRepository _repository;

    public GetCourseQueryHandler(ICourseRepository repository)
    {
        _repository = repository;
    }

    public async Task<CourseResponse?> HandleAsync(GetCourseQuery query, CancellationToken ct)
    {
        var course = await _repository.GetByIdAsync(query.CourseId, query.PartitionKey, ct);
        return CourseMapper.ToResponse(course);
    }
}
