using Carter;
using CourseLibrary.Course.Api.Configuration;
using CourseLibrary.Course.Api.Http;
using CourseLibrary.Course.Application.Operations.Courses.Delete;
using CourseLibrary.Platform.AspNetCore.RequestContext;

namespace CourseLibrary.Course.Api.Endpoints.Courses.DeleteCourse;

public sealed class DeleteCourseEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiV1Group("courses", "Courses");

        group.MapDelete("/{courseId}", async (
                string courseId,
                IRequestContext requestContext,
                IDeleteCourseOperation operation,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(requestContext.UserId))
                {
                    return Results.Unauthorized();
                }

                var result = await operation.ExecuteAsync(courseId, requestContext.UserId, cancellationToken);
                return result.ToHttpResult(deleted => deleted
                    ? Results.NoContent()
                    : Results.NotFound());
            })
            .WithName("DeleteCourse")
            .HasApiVersion(1.0);
    }
}
