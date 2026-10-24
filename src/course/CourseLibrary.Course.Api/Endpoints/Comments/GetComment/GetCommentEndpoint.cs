using Carter;
using CourseLibrary.Course.Api.Configuration;
using CourseLibrary.Course.Api.Http;
using CourseLibrary.Course.Application.UseCases.Comments.Get;

namespace CourseLibrary.Course.Api.Endpoints.Comments.GetComment;

public sealed class GetCommentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiV1Group("comments", "Comments");

        group.MapGet("/{commentId}/{courseId}", async (
                string commentId,
                string courseId,
                IGetCommentUseCase useCase,
                CancellationToken cancellationToken) =>
                (await useCase.ExecuteAsync(commentId, courseId, cancellationToken)).ToHttpResult())
            .WithName("GetComment")
            .HasApiVersion(1.0);
    }
}
