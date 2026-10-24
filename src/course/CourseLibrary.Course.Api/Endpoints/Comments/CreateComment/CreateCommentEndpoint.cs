using Carter;
using CourseLibrary.Course.Api.Configuration;
using CourseLibrary.Course.Api.Http;
using CourseLibrary.Course.Application.UseCases.Comments.Create;

namespace CourseLibrary.Course.Api.Endpoints.Comments.CreateComment;

public sealed class CreateCommentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiV1Group("comments", "Comments");

        group.MapPost("/", async (
                CreateCommentRequest request,
                ICreateCommentUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                var result = await useCase.ExecuteAsync(
                    request.CourseId,
                    request.AuthorId,
                    request.Content,
                    request.ParentCommentId,
                    cancellationToken);
                return result.ToHttpResult(comment =>
                    Results.CreatedAtRoute("GetComment", new
                    {
                        version = "1",
                        commentId = comment?.Id,
                        courseId = comment?.CourseId
                    }, comment));
            })
            .WithName("CreateComment")
            .HasApiVersion(1.0);
    }

    public sealed record CreateCommentRequest(
        string CourseId,
        string AuthorId,
        string Content,
        string? ParentCommentId);
}
