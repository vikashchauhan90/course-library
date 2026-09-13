using CourseLibrary.Domain.Entities;
using CourseLibrary.Application.Operations.Comments;
using CourseLibrary.Application.Operations.Discussions;
using CourseLibrary.Models.Course;

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
            course.Id.ToString(),
            course.Title,
            course.Description,
            course.AuthorId.ToString(),
            course.AuthorName,
            course.CreatedAt,
            course.UpdatedAt,
            course.RetiredAt,
            course.DeletedAt,
            comments ?? Array.Empty<CommentResponse>(),
            discussions ?? Array.Empty<DiscussionResponse>());
}
