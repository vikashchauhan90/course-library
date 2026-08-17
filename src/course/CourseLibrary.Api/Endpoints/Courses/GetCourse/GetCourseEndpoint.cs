using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Api.Endpoints.Courses.GetCourse;
using CourseLibrary.Application.Operations.Courses;
using CourseLibrary.Application.Operations.Courses.Get;
using MediatorForge.Abstractions;

namespace CourseLibrary.Api.Endpoints.Courses.GetCourse;

public sealed class GetCourseEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/courses")
            .WithTags("Courses");

        group.MapGet(
            "/{courseId}/{partitionKey}",
            async (
                HttpContext httpContext,
                IDispatcher dispatcher,
                string courseId,
                string partitionKey,
                ILogger<GetCourseEndpoint> logger) =>
            {
                var ct = httpContext.RequestAborted;

                logger.GettingCourse(courseId);

                var query = GetCourseMapper.ToQuery(courseId, partitionKey);

                var course = await dispatcher.QueryAsync<GetCourseQuery, CourseResponse?>(
                    query,
                    ct);

                if (course is null)
                {
                    logger.CourseNotFound(courseId);
                    return Results.NotFound();
                }

                logger.CourseRetrieved(courseId);
                return Results.Ok(course);
            })
            .WithName("GetCourse")
            .HasApiVersion(1.0);
    }
}
