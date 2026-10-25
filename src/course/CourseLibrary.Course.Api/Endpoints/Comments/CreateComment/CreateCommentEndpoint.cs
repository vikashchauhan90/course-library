using Carter;
using CourseLibrary.Course.Api.Configuration;
using CourseLibrary.Course.Api.Http;
using CourseLibrary.Course.Contracts.DTOs.Courses.Comments;
using CourseLibrary.Course.Application.UseCases.Comments.Create;

namespace CourseLibrary.Course.Api.Endpoints.Comments.CreateComment;

public sealed class CreateCommentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiV1Group("comments", "Comments");

        group.MapPost("/", async (
                CommentRequest request,
                ICreateCommentUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                var result = await useCase.ExecuteAsync(request, cancellationToken);
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
}
