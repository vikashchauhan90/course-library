using CourseLibrary.Application.Operations.Courses.Create;
using CourseLibrary.Models.Course;

namespace CourseLibrary.Api.Endpoints.Courses.CreateCourse;

public static class CreateCourseMapper
{
    public static CreateCourseCommand ToCommand(CreateCourseRequest request, string authorId)
    => new(request.Title, request.Description, authorId, request.AuthorName);
}
