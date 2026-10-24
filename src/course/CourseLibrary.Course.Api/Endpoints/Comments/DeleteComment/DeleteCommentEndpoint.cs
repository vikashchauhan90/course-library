using Carter;
using CourseLibrary.Course.Api.Configuration;
using CourseLibrary.Course.Api.Http;
using CourseLibrary.Course.Application.UseCases.Comments.Delete;

namespace CourseLibrary.Course.Api.Endpoints.Comments.DeleteComment;

public sealed class DeleteCommentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiV1Group("comments", "Comments");

        group.MapDelete("/{commentId}/{courseId}", async (
                string commentId,
                string courseId,
                IDeleteCommentUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                var result = await useCase.ExecuteAsync(commentId, courseId, cancellationToken);
                return result.ToHttpResult(deleted => deleted
                    ? Results.NoContent()
                    : Results.NotFound());
            })
            .WithName("DeleteComment")
            .HasApiVersion(1.0);
    }
}
