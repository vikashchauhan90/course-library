using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Operations.Comments;
using CourseLibrary.Application.Operations.Courses;
using CourseLibrary.Application.Operations.Courses.Search;
using CourseLibrary.Application.Operations.Discussions;
using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Models;
using MediatorForge.Abstractions;

namespace CourseLibrary.Api.Endpoints.Courses.GetCourses;

public sealed class GetCoursesEndpoint : ICarterModule
{
    public const string RouteName = "GetCourses";
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/courses")
            .WithTags("Courses");

        group.MapGet(
                "/",
                async (
                    HttpContext httpContext,
                    LinkGenerator linkGenerator,
                    IDispatcher dispatcher,
                    ILogger<GetCoursesEndpoint> logger,
                    ICommentRepository commentRepository,
                    IDiscussionRepository discussionRepository,
                    IRequestContext requestContext) =>
                {
                    var ct = httpContext.RequestAborted;

                    int pageSize = httpContext
                    .Request.Query.
                    TryGetValue("pageSize", out var pageSizeValues) &&
                    int.TryParse(pageSizeValues.FirstOrDefault(), out var parsedPageSize)
                        ? parsedPageSize
                        : 10; // Default page size

                    string? pageToken = httpContext.Request.Query["pageToken"];
                    string? searchTerm = httpContext.Request.Query["name"];
                    bool mine = bool.TryParse(httpContext.Request.Query["mine"], out var mineValue) && mineValue;
                    bool includeDeleted = bool.TryParse(httpContext.Request.Query["includeDeleted"], out var deletedValue) && deletedValue;
                    bool includeRetired = bool.TryParse(httpContext.Request.Query["includeRetired"], out var retiredValue) && retiredValue;
                    var authorId = mine ? requestContext.UserId : null;

                    var page = await dispatcher.QueryAsync<SearchCourseQuery, PageResult<CourseResponse>>(
                        new SearchCourseQuery(searchTerm, authorId, includeDeleted, includeRetired, pageSize, pageToken),
                        httpContext.RequestAborted);
                    logger.GettingAllCourses();

                    logger.CoursesRetrieved(page.Items.Count);
                    return Results.Ok(CourseHelper.GetCoursesResponse(linkGenerator, page));
                })
            .WithName(RouteName)
            .HasApiVersion(1.0);


    }
}