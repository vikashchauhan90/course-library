using CourseLibrary.Domain.Entities;

namespace CourseLibrary.Application.Operations.Discussions;

/// <summary>
/// Mapper for Discussion domain entity to response models.
/// </summary>
public static class DiscussionMapper
{
    public static DiscussionResponse ToResponse(Discussion discussion)
        => new(
            discussion.Id,
            discussion.CourseId,
            discussion.Title,
            discussion.Description,
            discussion.CreatedAt,
            discussion.UpdatedAt);

    public static DiscussionResponse? ToResponse(Discussion? discussion)
        => discussion is null ? null : ToResponse(discussion);

    public static IReadOnlyList<DiscussionResponse> ToResponses(IReadOnlyList<Discussion> discussions)
        => discussions.Select(ToResponse).ToList().AsReadOnly();
}
