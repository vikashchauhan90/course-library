using CourseLibrary.Application.Abstractions.Repositories;
using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Authors.Get;

public sealed class GetAuthorQueryHandler : IHandler<GetAuthorQuery, Domain.Entities.Author?>
{
    private readonly IAuthorRepository _repository;

    public GetAuthorQueryHandler(IAuthorRepository repository)
    {
        _repository = repository;
    }

    public Task<Domain.Entities.Author?> HandleAsync(GetAuthorQuery query, CancellationToken ct)
    {
        return _repository.GetByIdAsync(query.AuthorId, ct);
    }
}
