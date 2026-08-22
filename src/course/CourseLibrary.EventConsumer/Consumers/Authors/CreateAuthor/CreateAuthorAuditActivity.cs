using Castle.Core.Logging;
using CourseLibrary.Application.Operations.Authors.Create;
using CourseLibrary.Domain.Events;
using CourseLibrary.EventConsumer.Configuration.Observability.Traces;
using MediatorForge.Abstractions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.EventConsumer.Consumers.Authors.CreateAuthor;

internal sealed class CreateAuthorAuditActivity(
    ILogger<CreateAuthorAuditActivity> logger)
{
    [Function(nameof(CreateAuthorAuditActivity))]
    public async Task RunAsync(
        [ActivityTrigger] AuthorCreatedEvent authorEvent,
        ICommandDispatcher dispatcher,
        FunctionContext context,
        CancellationToken cancellationToken)
    {
        // Activity span for the entire activity
        using var activity = ActivitySources.EventConsumer.StartActivity(
            "activity.create-author-audit",
            ActivityKind.Internal);

        activity?.SetTag("audit.author_id", authorEvent.AuthorId);
        activity?.SetTag("audit.action", "CreateAuthor");
        try
        {
            logger.LogInformation(
            "Processing audit event for AuthorId {AuthorId}.",
            authorEvent.AuthorId);

            var command = new CreateAuthorAuditCommand(
                authorEvent.AuthorId,
                authorEvent.Name,
                authorEvent.Bio,
                authorEvent.Website,
                authorEvent.ActorId,
                authorEvent.OccurredAt);

            await dispatcher.SendAsync<
                CreateAuthorAuditCommand,
                Unit>(
                command,
                cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);

            logger.LogInformation(
                "Audit created successfully for AuthorId {AuthorId}",
                authorEvent.AuthorId);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogError(
               ex,
               "Failed to create audit for AuthorId {AuthorId}",
               authorEvent.AuthorId);

            throw;
        }
        
    }
}