using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Operations.Discussions;
using CourseLibrary.Domain.ValueObjects;
using CourseLibrary.Models.Course;

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
        var discussionId = (DiscussionId)Guid.Parse(query.DiscussionId);
        var courseId = (CourseId)Guid.Parse(query.CourseId);
        var discussion = await _repository.GetByIdAsync(discussionId, courseId, ct);
        return discussion is null ? null : DiscussionMapper.ToResponse(discussion);
    }
}
