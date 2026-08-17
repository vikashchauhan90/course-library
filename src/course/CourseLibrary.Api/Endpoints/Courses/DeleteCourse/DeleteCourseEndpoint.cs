using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Api.Endpoints.Courses.DeleteCourse;
using CourseLibrary.Application.Operations.Courses.Delete;
using MediatorForge.Abstractions;

namespace CourseLibrary.Api.Endpoints.Courses.DeleteCourse;

public sealed class DeleteCourseEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/courses")
            .WithTags("Courses");

        group.MapDelete(
            "/{courseId}/{partitionKey}",
            async (
                HttpContext httpContext,
                IDispatcher dispatcher,
                string courseId,
                string partitionKey,
                ILogger<DeleteCourseEndpoint> logger) =>
            {
                var ct = httpContext.RequestAborted;

                logger.DeletingCourse(courseId);

                var command = DeleteCourseMapper.ToCommand(courseId, partitionKey);

                var deleted = await dispatcher.SendAsync<DeleteCourseCommand, bool>(
                    command,
                    ct);

                if (deleted)
                {
                    logger.CourseDeleted(courseId);
                    return Results.NoContent();
                }

                logger.CourseNotFoundForDeletion(courseId);
                return Results.NotFound();
            })
            .WithName("DeleteCourse")
            .HasApiVersion(1.0);
    }
}
