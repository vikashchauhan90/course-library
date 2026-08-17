using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Api.Endpoints.Discussions.DeleteDiscussion;
using CourseLibrary.Application.Operations.Discussions.Delete;
using MediatorForge.Abstractions;

namespace CourseLibrary.Api.Endpoints.Discussions.DeleteDiscussion;

public sealed class DeleteDiscussionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/discussions")
            .WithTags("Discussions");

        group.MapDelete(
            "/{discussionId}/{courseId}",
            async (
                HttpContext httpContext,
                IDispatcher dispatcher,
                string discussionId,
                string courseId,
                ILogger<DeleteDiscussionEndpoint> logger) =>
            {
                var ct = httpContext.RequestAborted;

                logger.DeletingDiscussion(discussionId);

                var command = DeleteDiscussionMapper.ToCommand(discussionId, courseId);

                var deleted = await dispatcher.SendAsync<DeleteDiscussionCommand, bool>(
                    command,
                    ct);

                if (deleted)
                {
                    logger.DiscussionDeleted(discussionId);
                    return Results.NoContent();
                }

                logger.DiscussionNotFoundForDeletion(discussionId);
                return Results.NotFound();
            })
            .WithName("DeleteDiscussion")
            .HasApiVersion(1.0);
    }
}
