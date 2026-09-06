using CourseLibrary.Domain.Entities;
using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Operations.Authors;

namespace CourseLibrary.Application.Operations.Courses;

/// <summary>
/// Mapper for Course domain entity to response models.
/// </summary>
public static class CourseMapper
{
    public static CourseResponse ToResponse(Course course, AuthorResponse? author = null)
        => new(
            course.Id,
            course.Title,
            course.Description,
            course.AuthorId,
            course.CreatedAt,
            course.UpdatedAt,
            author);

    public static async Task<CourseResponse> ToResponseAsync(
        Course course,
        IAuthorRepository authorRepository,
        CancellationToken cancellationToken = default)
    {
        var author = await authorRepository.GetByIdAsync(course.AuthorId, cancellationToken);
        return ToResponse(course, author is null ? null : AuthorMapper.ToResponse(author));
    }

    public static IReadOnlyList<CourseResponse> ToResponses(IReadOnlyList<Course> courses)
        => courses.Select(course => ToResponse(course)).ToList().AsReadOnly();
}
