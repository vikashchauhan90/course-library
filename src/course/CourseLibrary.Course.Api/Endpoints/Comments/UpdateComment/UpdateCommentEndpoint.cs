using Carter;
using CourseLibrary.Course.Api.Configuration;
using CourseLibrary.Course.Api.Http;
using CourseLibrary.Course.Contracts.DTOs.Courses.Comments;
using CourseLibrary.Course.Application.UseCases.Comments.Update;

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
                IUpdateCommentUseCase useCase,
                CancellationToken cancellationToken) =>
                (await useCase.ExecuteAsync(
                    commentId,
                    courseId,
                    request.Content!,
                    cancellationToken)).ToHttpResult())
            .WithName("UpdateComment")
            .HasApiVersion(1.0);
    }
}
