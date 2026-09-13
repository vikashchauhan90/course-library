using CourseLibrary.Domain.Entities;
using CourseLibrary.Models.Course;

namespace CourseLibrary.Application.Operations.Discussions;

/// <summary>
/// Mapper for Discussion domain entity to response models.
/// </summary>
public static class DiscussionMapper
{
    public static DiscussionResponse ToResponse(Discussion discussion)
        => new(
            discussion.Id.ToString(),
            discussion.CourseId.ToString(),
            discussion.Title,
            discussion.Description,
            discussion.CreatedAt,
            discussion.UpdatedAt);

    public static IReadOnlyList<DiscussionResponse> ToResponses(IReadOnlyList<Discussion> discussions)
        => discussions.Select(ToResponse).ToList().AsReadOnly();
}
