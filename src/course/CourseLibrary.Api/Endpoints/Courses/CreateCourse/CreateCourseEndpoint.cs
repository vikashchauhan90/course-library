using Asp.Versioning;
using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Application.Operations.Courses;
using CourseLibrary.Application.Operations.Courses.Create;
using CourseLibrary.Models.Course;
using MediatorForge.Abstractions;
using CourseLibrary.Api.Configuration.OutputCache;

namespace CourseLibrary.Api.Endpoints.Courses.CreateCourse;

public sealed class CreateCourseEndpoint : ICarterModule
{
    public const string RouteName = "CreateCourse";
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/courses")
            .WithTags("Courses");

        group.MapPost(
            "/",
            async (
                HttpContext httpContext,
                 LinkGenerator linkGenerator,
                CreateCourseRequest request,
                IRequestContext requestContext,
                IDispatcher dispatcher,
                ILogger<CreateCourseEndpoint> logger) =>
            {
                var ct = httpContext.RequestAborted;
                var feature = httpContext.Features.Get<IApiVersioningFeature>();
                var apiVersion = feature?.RequestedApiVersion?.ToString() ?? "1";

                logger.CreatingCourse(request.Title);

                var actorId = requestContext.UserId;
                if (string.IsNullOrWhiteSpace(actorId))
                    return Results.Unauthorized();

                var command = CreateCourseMapper.ToCommand(request);

                var course = await dispatcher.SendAsync<CreateCourseCommand, CourseResponse>(
                    command,
                    ct);

                logger.CourseCreated(course.Id);
                var resource = CourseHelper.GetCourseResponse(linkGenerator, course, apiVersion);
                return Results.Created(resource.Links.First(x => x.Rel.Equals("self")).Href, resource);
            })
            .WithName(RouteName)
            .CacheOutput(OutputCachePolicies.Idempotency)
            .HasApiVersion(1.0);
    }
}
