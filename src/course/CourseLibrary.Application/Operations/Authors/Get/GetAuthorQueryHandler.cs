using CourseLibrary.Application.Abstractions.Repositories;
using MediatorForge.Abstractions;
using CourseLibrary.Application.Operations.Authors;

namespace CourseLibrary.Application.Operations.Authors.Get;

public sealed class GetAuthorQueryHandler : IHandler<GetAuthorQuery, AuthorResponse?>
{
    private readonly IAuthorRepository _repository;

    public GetAuthorQueryHandler(IAuthorRepository repository)
    {
        _repository = repository;
    }

    public async Task<AuthorResponse?> HandleAsync(GetAuthorQuery query, CancellationToken ct)
    {
        var author = await _repository.GetByIdAsync(query.AuthorId, ct);
        return author is null ? null : AuthorMapper.ToResponse(author);
    }
}
