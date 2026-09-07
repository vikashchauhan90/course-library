using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Application.Operations.Courses;
using CourseLibrary.Application.Operations.Courses.Get;
using CourseLibrary.Api.Endpoints.Courses;
using Asp.Versioning;
using MediatorForge.Abstractions;

namespace CourseLibrary.Api.Endpoints.Courses.GetCourse;

public sealed class GetCourseEndpoint : ICarterModule
{
    public const string RouteName = "GetCourse";
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/courses")
            .WithTags("Courses");

        group.MapGet(
            "/{courseId}",
            async (
                HttpContext httpContext,
                IDispatcher dispatcher,
                LinkGenerator linkGenerator,
                string courseId,
                ILogger<GetCourseEndpoint> logger) =>
            {
                var ct = httpContext.RequestAborted;

                logger.GettingCourse(courseId);

                var query = GetCourseMapper.ToQuery(courseId);

                var course = await dispatcher.QueryAsync<GetCourseQuery, CourseResponse?>(
                    query,
                    ct);

                if (course is null)
                {
                    logger.CourseNotFound(courseId);
                    return Results.NotFound();
                }

                logger.CourseRetrieved(courseId);
                var feature = httpContext.Features.Get<IApiVersioningFeature>();
                var apiVersion = feature?.RequestedApiVersion?.ToString() ?? "1";
                return Results.Ok(CourseHelper.GetCourseResponse(linkGenerator, course, apiVersion));
            })
            .WithName(RouteName)
            .HasApiVersion(1.0);
    }
}
