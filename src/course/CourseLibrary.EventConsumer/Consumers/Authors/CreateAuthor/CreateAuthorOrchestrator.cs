using CourseLibrary.Domain.Events;
using CourseLibrary.EventConsumer.Configuration.Observability.Traces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.EventConsumer.Consumers.Authors.CreateAuthor;

internal sealed class CreateAuthorOrchestrator
{
    [Function(nameof(CreateAuthorOrchestrator))]
    public static async Task RunAsync([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger<CreateAuthorOrchestrator>();

        // Handle replay without creating activities
        if (context.IsReplaying)
        {
            logger.LogDebug("Replaying orchestration {InstanceId}", context.InstanceId);
            var replayEvent = context.GetInput<AuthorCreatedEvent>();
            await context.CallActivityAsync(nameof(CreateAuthorAuditActivity), replayEvent);
            return;
        }

        using var orchestrationActivity = ActivitySources.EventConsumer.StartActivity(
            "orchestration.create-author",
            ActivityKind.Internal);

        try
        {
            orchestrationActivity?.SetTag("orchestration.instance_id", context.InstanceId);
            orchestrationActivity?.SetTag("orchestration.name", nameof(CreateAuthorOrchestrator));

            logger.LogInformation(
                "Starting CreateAuthor orchestration. InstanceId: {InstanceId}.",
                context.InstanceId);

            var authorEvent = context.GetInput<AuthorCreatedEvent>();

            if (authorEvent is null)
            {
                orchestrationActivity?.SetStatus(ActivityStatusCode.Error, "Null event input");
                logger.LogError(
                    "CreateAuthor orchestration {InstanceId} did not receive an AuthorCreatedEvent.",
                    context.InstanceId);
                throw new InvalidOperationException("AuthorCreatedEvent was not provided.");
            }

            orchestrationActivity?.SetTag("event.author_id", authorEvent.AuthorId);
            orchestrationActivity?.SetTag("event.type", authorEvent.GetType().Name);

            await context.CallActivityAsync(nameof(CreateAuthorAuditActivity), authorEvent);

            orchestrationActivity?.SetStatus(ActivityStatusCode.Ok);
            logger.LogInformation(
                "CreateAuthor orchestration {InstanceId} completed for AuthorId {AuthorId}.",
                context.InstanceId, authorEvent.AuthorId);
        }
        catch (Exception ex)
        {
            orchestrationActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogError(ex, "CreateAuthor orchestration {InstanceId} failed.", context.InstanceId);
            throw;
        }
    }
}