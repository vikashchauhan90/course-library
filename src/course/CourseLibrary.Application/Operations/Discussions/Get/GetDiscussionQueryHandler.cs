using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;

namespace CourseLibrary.Application.Operations.Discussions.Get;

public sealed class GetDiscussionQueryHandler : IHandler<GetDiscussionQuery, CourseLibrary.Domain.Entities.Discussion?>
{
    private readonly IDiscussionRepository _repository;

    public GetDiscussionQueryHandler(IDiscussionRepository repository)
    {
        _repository = repository;
    }

    public Task<CourseLibrary.Domain.Entities.Discussion?> HandleAsync(GetDiscussionQuery query, CancellationToken ct)
        => _repository.GetByIdAsync(query.DiscussionId, query.CourseId, ct);
}
