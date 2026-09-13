using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Application.Operations.Courses;
using CourseLibrary.Application.Operations.Courses.Update;
using CourseLibrary.Api.Endpoints.Courses;
using Asp.Versioning;
using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Models.Course;
using MediatorForge.Abstractions;
using CourseLibrary.Api.Configuration.OutputCache;

namespace CourseLibrary.Api.Endpoints.Courses.UpdateCourse;

public sealed class UpdateCourseEndpoint : ICarterModule
{
    public const string RouteName = "UpdateCourse";
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/courses")
            .WithTags("Courses");

        group.MapPut(
            "/{courseId}",
            async (
                HttpContext httpContext,
                IDispatcher dispatcher,
                LinkGenerator linkGenerator,
                string courseId,
                UpdateCourseRequest request,
                IRequestContext requestContext,
                ILogger<UpdateCourseEndpoint> logger) =>
            {
                var ct = httpContext.RequestAborted;

                logger.UpdatingCourse(courseId);

                if (string.IsNullOrWhiteSpace(requestContext.UserId))
                    return Results.Forbid();

                var command = UpdateCourseMapper.ToCommand(courseId, request);

                var course = await dispatcher.SendAsync<UpdateCourseCommand, CourseResponse?>(
                    command,
                    ct);

                if (course is null)
                {
                    return Results.NotFound();
                }
                logger.CourseUpdated(courseId);
                var feature = httpContext.Features.Get<IApiVersioningFeature>();
                var apiVersion = feature?.RequestedApiVersion?.ToString() ?? "1";
                return Results.Ok(CourseHelper.GetCourseResponse(linkGenerator, course, apiVersion));
            })
            .WithName(RouteName)
            .CacheOutput(OutputCachePolicies.Idempotency)
            .HasApiVersion(1.0);
    }
}
