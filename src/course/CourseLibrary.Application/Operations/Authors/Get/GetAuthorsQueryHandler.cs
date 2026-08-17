using CourseLibrary.Application.Abstractions.Repositories;
using MediatorForge.Abstractions;
using CourseLibrary.Application.Operations.Authors;

namespace CourseLibrary.Application.Operations.Authors.Get;

public sealed class GetAuthorsQueryHandler : IHandler<GetAuthorsQuery, IReadOnlyList<AuthorResponse>>
{
    private readonly IAuthorRepository _repository;

    public GetAuthorsQueryHandler(IAuthorRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AuthorResponse>> HandleAsync(GetAuthorsQuery query, CancellationToken ct)
    {
        var authors = await _repository.GetAllAsync(ct);
        return AuthorMapper.ToResponses(authors);
    }
}
