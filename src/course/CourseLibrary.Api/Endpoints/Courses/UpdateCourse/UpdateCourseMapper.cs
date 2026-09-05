using CourseLibrary.Application.Operations.Courses.Update;

namespace CourseLibrary.Api.Endpoints.Courses.UpdateCourse;

public static class UpdateCourseMapper
{
    public static UpdateCourseCommand ToCommand(string courseId, string authorId, UpdateCourseRequest request)
        => new(courseId, request.Title, request.Description, authorId);
}
