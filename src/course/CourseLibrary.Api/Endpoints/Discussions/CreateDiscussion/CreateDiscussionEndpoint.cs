using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Api.Endpoints.Discussions.CreateDiscussion;
using CourseLibrary.Application.Operations.Discussions;
using CourseLibrary.Application.Operations.Discussions.Create;
using CourseLibrary.Domain.Entities;
using MediatorForge.Abstractions;

namespace CourseLibrary.Api.Endpoints.Discussions.CreateDiscussion;

public sealed class CreateDiscussionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/discussions")
            .WithTags("Discussions");

        group.MapPost(
            "/",
            async (
                HttpContext httpContext,
                CreateDiscussionRequest request,
                IDispatcher dispatcher,
                ILogger<CreateDiscussionEndpoint> logger) =>
            {
                var ct = httpContext.RequestAborted;

                logger.CreatingDiscussion(request.Title);

                var command = CreateDiscussionMapper.ToCommand(request);

                var discussion = await dispatcher.SendAsync<CreateDiscussionCommand, DiscussionResponse>(
                    command,
                    ct);

                logger.DiscussionCreated(discussion.Id);

                return Results.Created(
                    $"/api/v1/discussions/{discussion.Id}/{discussion.CourseId}",
                    discussion);
            })
            .WithName("CreateDiscussion")
            .HasApiVersion(1.0);
    }
}
