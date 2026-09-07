using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Api.Endpoints.Comments.GetComment;
using CourseLibrary.Application.Operations.Comments;
using CourseLibrary.Application.Operations.Comments.Get;
using CourseLibrary.Models.Course;
using MediatorForge.Abstractions;

namespace CourseLibrary.Api.Endpoints.Comments.GetComment;

public sealed class GetCommentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/comments")
            .WithTags("Comments");

        group.MapGet(
            "/{commentId}/{courseId}",
            async (
                HttpContext httpContext,
                IDispatcher dispatcher,
                string commentId,
                string courseId,
                LinkGenerator linkGenerator,
                ILogger<GetCommentEndpoint> logger) =>
            {
                var ct = httpContext.RequestAborted;

                logger.GettingComment(commentId);

                var query = GetCommentMapper.ToQuery(commentId, courseId);

                var comment = await dispatcher.QueryAsync<GetCommentQuery, CommentResponse?>(
                    query,
                    ct);

                if (comment is null)
                {
                    logger.CommentNotFound(commentId);
                    return Results.NotFound();
                }

                logger.CommentRetrieved(commentId);
                return Results.Ok(CommentHalHelper.ToResource(
                    linkGenerator,
                    comment));
            })
            .WithName("GetComment")
            .HasApiVersion(1.0);
    }
}
