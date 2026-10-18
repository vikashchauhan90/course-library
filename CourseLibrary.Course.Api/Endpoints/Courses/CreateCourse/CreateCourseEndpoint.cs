using Carter;
using CourseLibrary.Course.Api.Configuration;
using CourseLibrary.Course.Api.Http;
using CourseLibrary.Course.Application.Operations.Courses.Create;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Platform.AspNetCore.RequestContext;

namespace CourseLibrary.Course.Api.Endpoints.Courses.CreateCourse;

public sealed class CreateCourseEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiV1Group("courses", "Courses");

        group.MapPost("/", async (
                CreateCourseRequest request,
                IRequestContext requestContext,
                ICreateCourseOperation operation,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(requestContext.UserId))
                {
                    return Results.Unauthorized();
                }

                var result = await operation.ExecuteAsync(request, requestContext.UserId, cancellationToken);
                return result.ToHttpResult(course =>
                    Results.CreatedAtRoute("GetCourse", new { version = "1", courseId = course?.Id }, course));
            })
            .WithName("CreateCourse")
            .HasApiVersion(1.0);
    }
}
