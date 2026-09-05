using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Operations.Courses;

namespace CourseLibrary.Api.Endpoints.Courses.SearchCourses;

public sealed class SearchCoursesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/courses")
            .WithTags("Courses");

        group.MapGet(
                "/search",
                async (
                    HttpContext httpContext,
                    ICourseRepository repository,
                    string? q,
                    int? pageSize) =>
                {
                    var query = q?.Trim() ?? string.Empty;
                    var results = await repository.SearchAsync(
                        query,
                        Math.Clamp(pageSize ?? 20, 1, 100),
                        continuationToken: null,
                        httpContext.RequestAborted);

                    return Results.Ok(CourseMapper.ToResponses(results));
                })
            .WithName("SearchCourses")
            .HasApiVersion(1.0);

        group.MapGet(
                "/mine",
                async (
                    HttpContext httpContext,
                    ICourseRepository repository,
                    CourseLibrary.Application.Abstractions.RequestContext.IRequestContext requestContext) =>
                {
                    if (string.IsNullOrWhiteSpace(requestContext.UserId))
                        return Results.Unauthorized();

                    var results = await repository.GetByAuthorAsync(
                        requestContext.UserId,
                        httpContext.RequestAborted);

                    return Results.Ok(CourseMapper.ToResponses(results));
                })
            .WithName("GetMyCourses")
            .HasApiVersion(1.0);
    }
}