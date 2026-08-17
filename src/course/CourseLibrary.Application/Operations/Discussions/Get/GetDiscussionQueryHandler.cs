using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Operations.Discussions;

namespace CourseLibrary.Application.Operations.Discussions.Get;

public sealed class GetDiscussionQueryHandler : IHandler<GetDiscussionQuery, DiscussionResponse?>
{
    private readonly IDiscussionRepository _repository;

    public GetDiscussionQueryHandler(IDiscussionRepository repository)
    {
        _repository = repository;
    }

    public async Task<DiscussionResponse?> HandleAsync(GetDiscussionQuery query, CancellationToken ct)
    {
        var discussion = await _repository.GetByIdAsync(query.DiscussionId, query.CourseId, ct);
        return DiscussionMapper.ToResponse(discussion);
    }
}
