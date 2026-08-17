using CourseLibrary.Application.Operations.Courses.Create;

namespace CourseLibrary.Api.Endpoints.Courses.CreateCourse;

public static class CreateCourseMapper
{
    public static CreateCourseCommand ToCommand(CreateCourseRequest request)
        => new(request.Title, request.Description, request.AuthorId);
}
