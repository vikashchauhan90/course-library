using CourseLibrary.Domain.Entities;
using CourseLibrary.Application.Operations.Comments;
using CourseLibrary.Application.Operations.Discussions;

namespace CourseLibrary.Application.Operations.Courses;

/// <summary>
/// Mapper for Course domain entity to response models.
/// </summary>
public static class CourseMapper
{
    public static CourseResponse ToResponse(
        Course course,
        IReadOnlyList<CommentResponse>? comments = null,
        IReadOnlyList<DiscussionResponse>? discussions = null)
        => new(
            course.Id,
            course.Title,
            course.Description,
            course.AuthorId,
            course.AuthorName,
            course.CreatedAt,
            course.UpdatedAt,
            comments ?? Array.Empty<CommentResponse>(),
            discussions ?? Array.Empty<DiscussionResponse>());
}
