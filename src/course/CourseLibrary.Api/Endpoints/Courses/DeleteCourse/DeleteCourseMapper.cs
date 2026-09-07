using CourseLibrary.Application.Operations.Courses.Delete;

namespace CourseLibrary.Api.Endpoints.Courses.DeleteCourse;

public static class DeleteCourseMapper
{
    public static DeleteCourseCommand ToCommand(string courseId)
        => new(courseId);
}
