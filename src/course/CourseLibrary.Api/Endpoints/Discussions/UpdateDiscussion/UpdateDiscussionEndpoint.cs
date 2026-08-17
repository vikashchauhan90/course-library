using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Api.Endpoints.Discussions.UpdateDiscussion;
using CourseLibrary.Application.Operations.Discussions;
using CourseLibrary.Application.Operations.Discussions.Update;
using MediatorForge.Abstractions;

namespace CourseLibrary.Api.Endpoints.Discussions.UpdateDiscussion;

public sealed class UpdateDiscussionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/discussions")
            .WithTags("Discussions");

        group.MapPut(
            "/{discussionId}/{courseId}",
            async (
                HttpContext httpContext,
                IDispatcher dispatcher,
                string discussionId,
                string courseId,
                UpdateDiscussionRequest request,
                ILogger<UpdateDiscussionEndpoint> logger) =>
            {
                var ct = httpContext.RequestAborted;

                logger.UpdatingDiscussion(discussionId);

                var command = UpdateDiscussionMapper.ToCommand(discussionId, courseId, request);

                var discussion = await dispatcher.SendAsync<UpdateDiscussionCommand, DiscussionResponse>(
                    command,
                    ct);

                logger.DiscussionUpdated(discussionId);
                return Results.Ok(discussion);
            })
            .WithName("UpdateDiscussion")
            .HasApiVersion(1.0);
    }
}
