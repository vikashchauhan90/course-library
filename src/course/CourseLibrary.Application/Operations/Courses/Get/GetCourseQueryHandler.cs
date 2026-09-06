using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Operations.Courses;

namespace CourseLibrary.Application.Operations.Courses.Get;

public sealed class GetCourseQueryHandler : IHandler<GetCourseQuery, CourseResponse?>
{
    private readonly ICourseRepository _repository;
    private readonly IAuthorRepository _authorRepository;

    public GetCourseQueryHandler(ICourseRepository repository, IAuthorRepository authorRepository)
    {
        _repository = repository;
        _authorRepository = authorRepository;
    }

    public async Task<CourseResponse?> HandleAsync(GetCourseQuery query, CancellationToken ct)
    {
        var course = await _repository.GetByIdAsync(query.CourseId, query.PartitionKey, ct);
        return course is null ? null : await CourseMapper.ToResponseAsync(course, _authorRepository, ct);
    }
}
