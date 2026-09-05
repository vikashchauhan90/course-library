using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Api.Endpoints.Courses.UpdateCourse;
using CourseLibrary.Application.Operations.Courses;
using CourseLibrary.Application.Operations.Courses.Update;
using CourseLibrary.Application.Abstractions.RequestContext;
using MediatorForge.Abstractions;

namespace CourseLibrary.Api.Endpoints.Courses.UpdateCourse;

public sealed class UpdateCourseEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/courses")
            .WithTags("Courses");

        group.MapPut(
            "/{courseId}/{partitionKey}",
            async (
                HttpContext httpContext,
                IDispatcher dispatcher,
                string courseId,
                string partitionKey,
                UpdateCourseRequest request,
                IRequestContext requestContext,
                ILogger<UpdateCourseEndpoint> logger) =>
            {
                var ct = httpContext.RequestAborted;

                logger.UpdatingCourse(courseId);

                if (string.IsNullOrWhiteSpace(requestContext.UserId) ||
                    !string.Equals(requestContext.UserId, partitionKey, StringComparison.Ordinal))
                    return Results.Forbid();

                var command = new UpdateCourseCommand(
                    courseId,
                    request.Title,
                    request.Description,
                    partitionKey);

                var course = await dispatcher.SendAsync<UpdateCourseCommand, CourseResponse>(
                    command,
                    ct);

                logger.CourseUpdated(courseId);
                return Results.Ok(course);
            })
            .WithName("UpdateCourse")
            .HasApiVersion(1.0);
    }
}
