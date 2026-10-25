using Carter;
using CourseLibrary.Course.Api.Configuration;
using CourseLibrary.Course.Api.Http;
using CourseLibrary.Course.Contracts.DTOs.Courses.Discussions;
using CourseLibrary.Course.Application.UseCases.Discussions.Create;

namespace CourseLibrary.Course.Api.Endpoints.Discussions.CreateDiscussion;

public sealed class CreateDiscussionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiV1Group("discussions", "Discussions");

        group.MapPost("/", async (
                DiscussionRequest request,
                ICreateDiscussionUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                var result = await useCase.ExecuteAsync(request, cancellationToken);
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

}
