using Asp.Versioning;
using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Application.Operations.Courses;
using CourseLibrary.Application.Operations.Courses.Retire;
using CourseLibrary.Api.Endpoints.Courses;
using MediatorForge.Abstractions;
using CourseLibrary.Models.Course;
using CourseLibrary.Api.Configuration.OutputCache;

namespace CourseLibrary.Api.Endpoints.Courses.RetireCourse;

public sealed class RetireCourseEndpoint : ICarterModule
{
    public const string RouteName = "RetireCourse";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/courses").WithTags("Courses");

        group.MapPost(
                "/{courseId}/retire",
                async (
                    HttpContext httpContext,
                    IDispatcher dispatcher,
                    LinkGenerator linkGenerator,
                    string courseId,
                    IRequestContext requestContext) =>
                {
                    if (string.IsNullOrWhiteSpace(requestContext.UserId))
                        return Results.Forbid();

                    var course = await dispatcher.SendAsync<RetireCourseCommand, CourseResponse?>(
                        new RetireCourseCommand(courseId),
                        httpContext.RequestAborted);

                    if (course is null)
                        return Results.NotFound();

                    var feature = httpContext.Features.Get<IApiVersioningFeature>();
                    var apiVersion = feature?.RequestedApiVersion?.ToString() ?? "1";
                    return Results.Ok(CourseHelper.GetCourseResponse(linkGenerator, course, apiVersion));
                })
            .WithName(RouteName)
            .CacheOutput(OutputCachePolicies.Idempotency)
            .HasApiVersion(1.0);
    }
}