using CourseLibrary.Domain.Events;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.EventConsumer.Consumers.Authors.CreateAuthor;

internal sealed class CreateAuthorOrchestrator
{
    [Function(nameof(CreateAuthorOrchestrator))]
    public static async Task RunAsync(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger =
            context.CreateReplaySafeLogger<CreateAuthorOrchestrator>();

        logger.LogInformation(
            "Starting CreateAuthor orchestration. InstanceId: {InstanceId}.",
            context.InstanceId);

        var courseEvent =
            context.GetInput<AuthorCreatedEvent>();

        if (courseEvent is null)
        {
            logger.LogError(
                "CreateAuthor orchestration {InstanceId} did not receive an AuthorCreatedEvent.",
                context.InstanceId);

            throw new InvalidOperationException(
                "AuthorCreatedEvent was not provided.");
        }

        logger.LogInformation(
            "Processing AuthorCreatedEvent for AuthorId {AuthorId}.",
            courseEvent.AuthorId);

        await context.CallActivityAsync(
            nameof(CreateAuthorAuditActivity),
            courseEvent);

        logger.LogInformation(
            "CreateAuthor orchestration {InstanceId} completed for AuthorId {AuthorId}.",
            context.InstanceId,
            courseEvent.AuthorId);
    }
}