using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Api.Endpoints.Courses.UpdateCourse;
using CourseLibrary.Application.Operations.Courses.Update;
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
                ILogger<UpdateCourseEndpoint> logger) =>
            {
                var ct = httpContext.RequestAborted;

                logger.UpdatingCourse(courseId);

                var command = UpdateCourseMapper.ToCommand(courseId, request);

                var course = await dispatcher.SendAsync<UpdateCourseCommand, Domain.Entities.Course>(
                    command,
                    ct);

                logger.CourseUpdated(courseId);
                return Results.Ok(course);
            })
            .WithName("UpdateCourse")
            .HasApiVersion(1.0);
    }
}
