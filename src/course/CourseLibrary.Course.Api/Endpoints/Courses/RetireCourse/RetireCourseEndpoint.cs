using Carter;
using CourseLibrary.Course.Api.Configuration;
using CourseLibrary.Course.Api.Http;
using CourseLibrary.Course.Application.Operations.Courses.Retire;
using CourseLibrary.Platform.AspNetCore.RequestContext;

namespace CourseLibrary.Course.Api.Endpoints.Courses.RetireCourse;

public sealed class RetireCourseEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiV1Group("courses", "Courses");

        group.MapPost("/{courseId}/retire", async (
                string courseId,
                IRequestContext requestContext,
                IRetireCourseOperation operation,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(requestContext.UserId))
                {
                    return Results.Unauthorized();
                }

                return (await operation.ExecuteAsync(
                    courseId,
                    requestContext.UserId,
                    cancellationToken)).ToHttpResult();
            })
            .WithName("RetireCourse")
            .HasApiVersion(1.0);
    }
}
