using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Api.Endpoints.Discussions.GetDiscussion;
using CourseLibrary.Application.Operations.Discussions.Get;
using MediatorForge.Abstractions;

namespace CourseLibrary.Api.Endpoints.Discussions.GetDiscussion;

public sealed class GetDiscussionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/discussions")
            .WithTags("Discussions");

        group.MapGet(
            "/{discussionId}/{courseId}",
            async (
                HttpContext httpContext,
                IDispatcher dispatcher,
                string discussionId,
                string courseId,
                ILogger<GetDiscussionEndpoint> logger) =>
            {
                var ct = httpContext.RequestAborted;

                logger.GettingDiscussion(discussionId);

                var query = GetDiscussionMapper.ToQuery(discussionId, courseId);

                var discussion = await dispatcher.QueryAsync<GetDiscussionQuery, Domain.Entities.Discussion?>(
                    query,
                    ct);

                if (discussion is null)
                {
                    logger.DiscussionNotFound(discussionId);
                    return Results.NotFound();
                }

                logger.DiscussionRetrieved(discussionId);
                return Results.Ok(discussion);
            })
            .WithName("GetDiscussion")
            .HasApiVersion(1.0);
    }
}
