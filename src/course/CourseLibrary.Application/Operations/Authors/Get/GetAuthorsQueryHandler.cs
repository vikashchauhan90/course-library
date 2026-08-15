using CourseLibrary.Application.Abstractions.Repositories;
using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Authors.Get;

public sealed class GetAuthorsQueryHandler : IHandler<GetAuthorsQuery, IReadOnlyList<Domain.Entities.Author>>
{
    private readonly IAuthorRepository _repository;

    public GetAuthorsQueryHandler(IAuthorRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<Domain.Entities.Author>> HandleAsync(GetAuthorsQuery query, CancellationToken ct)
    {
        return _repository.GetAllAsync(ct);
    }
}
