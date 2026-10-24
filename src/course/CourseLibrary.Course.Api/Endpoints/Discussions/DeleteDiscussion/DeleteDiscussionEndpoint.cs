using Carter;
using CourseLibrary.Course.Api.Configuration;
using CourseLibrary.Course.Api.Http;
using CourseLibrary.Course.Application.UseCases.Discussions.Delete;

namespace CourseLibrary.Course.Api.Endpoints.Discussions.DeleteDiscussion;

public sealed class DeleteDiscussionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiV1Group("discussions", "Discussions");

        group.MapDelete("/{discussionId}/{courseId}", async (
                string discussionId,
                string courseId,
                IDeleteDiscussionUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                var result = await useCase.ExecuteAsync(discussionId, courseId, cancellationToken);
                return result.ToHttpResult(deleted => deleted
                    ? Results.NoContent()
                    : Results.NotFound());
            })
            .WithName("DeleteDiscussion")
            .HasApiVersion(1.0);
    }
}
