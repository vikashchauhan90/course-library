using CourseLibrary.Application.Operations.Courses.Get;

namespace CourseLibrary.Api.Endpoints.Courses.GetCourse;

public static class GetCourseMapper
{
    public static GetCourseQuery ToQuery(string courseId, string partitionKey)
        => new(courseId, partitionKey);
}
