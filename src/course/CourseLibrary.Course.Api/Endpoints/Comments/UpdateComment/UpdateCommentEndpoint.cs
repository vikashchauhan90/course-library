using Carter;
using CourseLibrary.Course.Api.Configuration;
using CourseLibrary.Course.Api.Http;
using CourseLibrary.Course.Application.Operations.Comments.Update;

namespace CourseLibrary.Course.Api.Endpoints.Comments.UpdateComment;

public sealed class UpdateCommentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiV1Group("comments", "Comments");

        group.MapPut("/{commentId}/{courseId}", async (
                string commentId,
                string courseId,
                UpdateCommentRequest request,
                IUpdateCommentOperation operation,
                CancellationToken cancellationToken) =>
                (await operation.ExecuteAsync(
                    commentId,
                    courseId,
                    request.Content,
                    cancellationToken)).ToHttpResult())
            .WithName("UpdateComment")
            .HasApiVersion(1.0);
    }

    public sealed record UpdateCommentRequest(string Content);
}
