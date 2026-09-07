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
                    string? searchTerm = httpContext.Request.Query["q"];
                    bool mine = bool.TryParse(httpContext.Request.Query["mine"], out var mineValue) && mineValue;
                    bool includeDeleted = bool.TryParse(httpContext.Request.Query["includeDeleted"], out var deletedValue) && deletedValue;
                    bool includeRetired = bool.TryParse(httpContext.Request.Query["includeRetired"], out var retiredValue) && retiredValue;
                    var authorId = mine ? requestContext.UserId : null;

                    var page = await dispatcher.QueryAsync<SearchCourseQuery, PageResult<CourseResponse>>(
                        new SearchCourseQuery(searchTerm, authorId, includeDeleted, includeRetired, pageSize, pageToken),
                        httpContext.RequestAborted);
                    logger.GettingAllCourses();

                    var responses = await Task.WhenAll(page.Items.Select(course =>
                        ToResponseAsync(course, commentRepository, discussionRepository, httpContext.RequestAborted)));

                    var responsePage = new PageResult<CourseResponse>(
                        responses,
                        page.ContinuationToken,
                        page.HasMore);
                    logger.CoursesRetrieved(responsePage.Items.Count);
                    return Results.Ok(CourseHelper.GetCoursesResponse(linkGenerator, responsePage));
                })
            .WithName(RouteName)
            .HasApiVersion(1.0);


    }

    private static async Task<CourseResponse> ToResponseAsync(
        CourseResponse course,
        ICommentRepository commentRepository,
        IDiscussionRepository discussionRepository,
        CancellationToken cancellationToken)
    {
        var commentsTask = commentRepository.GetByCourseAsync(course.Id, cancellationToken);
        var discussionsTask = discussionRepository.GetByCourseAsync(course.Id, cancellationToken);
        await Task.WhenAll(commentsTask, discussionsTask);

        return course with
        {
            Comments = CommentMapper.ToResponses(await commentsTask),
            Discussions = DiscussionMapper.ToResponses(await discussionsTask)
        };
    }
}