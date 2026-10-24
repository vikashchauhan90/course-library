using Carter;
using CourseLibrary.Course.Api.Configuration;
using CourseLibrary.Course.Api.Http;
using CourseLibrary.Course.Application.UseCases.Discussions.Create;

namespace CourseLibrary.Course.Api.Endpoints.Discussions.CreateDiscussion;

public sealed class CreateDiscussionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiV1Group("discussions", "Discussions");

        group.MapPost("/", async (
                CreateDiscussionRequest request,
                ICreateDiscussionUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                var result = await useCase.ExecuteAsync(
                    request.CourseId,
                    request.Title,
                    request.Description,
                    cancellationToken);
                return result.ToHttpResult(discussion =>
                    Results.CreatedAtRoute("GetDiscussion", new
                    {
                        version = "1",
                        discussionId = discussion?.Id,
                        courseId = discussion?.CourseId
                    }, discussion));
            })
            .WithName("CreateDiscussion")
            .HasApiVersion(1.0);
    }

    public sealed record CreateDiscussionRequest(string CourseId, string Title, string Description);
}
