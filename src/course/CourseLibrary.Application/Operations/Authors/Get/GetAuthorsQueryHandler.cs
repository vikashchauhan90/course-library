using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Domain.Models;
using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Authors.Get;

public sealed class GetAuthorsQueryHandler : IHandler<GetAuthorsQuery, PageResult<AuthorResponse>>
{
    private readonly IAuthorRepository _repository;

    public GetAuthorsQueryHandler(IAuthorRepository repository)
    {
        _repository = repository;
    }

    public async Task<PageResult<AuthorResponse>> HandleAsync(GetAuthorsQuery query, CancellationToken ct)
    {
        var authors = await _repository.QueryPageAsync(query.PageSize, query.PageToken, ct);
        return authors.Map(AuthorMapper.ToResponse);
    }
}
