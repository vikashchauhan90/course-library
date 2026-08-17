using CourseLibrary.Domain.Entities;

namespace CourseLibrary.Application.Operations.Courses;

/// <summary>
/// Mapper for Course domain entity to response models.
/// </summary>
public static class CourseMapper
{
    public static CourseResponse ToResponse(Course course)
        => new(
            course.Id,
            course.Title,
            course.Description,
            course.AuthorId,
            course.CreatedAt,
            course.UpdatedAt);

    public static IReadOnlyList<CourseResponse> ToResponses(IReadOnlyList<Course> courses)
        => courses.Select(ToResponse).ToList().AsReadOnly();
}
