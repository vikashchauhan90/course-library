using CourseLibrary.Domain.Events;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.EventConsumer.Consumers.Authors.CreateAuthor;

internal sealed class CreateAuthorAuditActivity(
    ILogger<CreateAuthorAuditActivity> logger)
{
    [Function(nameof(CreateAuthorAuditActivity))]
    public async Task RunAsync(
        [ActivityTrigger] AuthorCreatedEvent authorEvent,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing audit event for AuthorId {AuthorId}.",
            authorEvent.AuthorId);


        // TODO: Persist the audit event.
        //
        // var command = new CreateAuthorAuditCommand(
        //     authorEvent.AuthorId,
        //     authorEvent.ActorId,
        //     authorEvent.OccurredAt);
        //
        // await sender.Send(
        //     command,
        //     cancellationToken);

        await Task.CompletedTask;
    }
}