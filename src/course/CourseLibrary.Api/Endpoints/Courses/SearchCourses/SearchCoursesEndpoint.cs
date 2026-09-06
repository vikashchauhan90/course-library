using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Operations.Courses;
using CourseLibrary.Domain.Models;

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
                    IAuthorRepository authorRepository,
                    string? q,
                    int? pageSize,
                    string? continuationToken) =>
                {
                    var query = q?.Trim() ?? string.Empty;
                    var page = await repository.SearchAsync(
                        query,
                        Math.Clamp(pageSize ?? 20, 1, 100),
                        continuationToken,
                        httpContext.RequestAborted);

                    var responses = await Task.WhenAll(
                        page.Items.Select(course => CourseMapper.ToResponseAsync(
                            course,
                            authorRepository,
                            httpContext.RequestAborted)));

                    return Results.Ok(new PageResult<CourseResponse>(
                        responses,
                        page.ContinuationToken,
                        page.HasMore));
                })
            .WithName("SearchCourses")
            .HasApiVersion(1.0);

        group.MapGet(
                "/mine",
                async (
                    HttpContext httpContext,
                    ICourseRepository repository,
                    IAuthorRepository authorRepository,
                    CourseLibrary.Application.Abstractions.RequestContext.IRequestContext requestContext,
                    int? pageSize,
                    string? continuationToken) =>
                {
                    if (string.IsNullOrWhiteSpace(requestContext.UserId))
                        return Results.Unauthorized();

                    var page = await repository.GetByAuthorAsync(
                        requestContext.UserId,
                        Math.Clamp(pageSize ?? 20, 1, 100),
                        continuationToken,
                        httpContext.RequestAborted);

                    var responses = await Task.WhenAll(
                        page.Items.Select(course => CourseMapper.ToResponseAsync(
                            course,
                            authorRepository,
                            httpContext.RequestAborted)));

                    return Results.Ok(new PageResult<CourseResponse>(
                        responses,
                        page.ContinuationToken,
                        page.HasMore));
                })
            .WithName("GetMyCourses")
            .HasApiVersion(1.0);
    }
}