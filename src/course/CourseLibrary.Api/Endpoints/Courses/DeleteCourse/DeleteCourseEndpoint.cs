using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Api.Endpoints.Courses.DeleteCourse;
using CourseLibrary.Application.Operations.Courses.Delete;
using CourseLibrary.Application.Abstractions.RequestContext;
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
                IRequestContext requestContext,
                ILogger<DeleteCourseEndpoint> logger) =>
            {
                var ct = httpContext.RequestAborted;

                if (string.IsNullOrWhiteSpace(requestContext.UserId) ||
                    !string.Equals(requestContext.UserId, partitionKey, StringComparison.Ordinal))
                    return Results.Forbid();

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
