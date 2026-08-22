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
    public static async Task RunAsync(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        // Get parent activity from the consumer
        var parentContext = Activity.Current?.Context ?? default;

        // MAIN ACTIVITY: Orchestration
        using var orchestrationActivity = ActivitySources.EventConsumer.StartActivity(
            "orchestration.create-author",
            ActivityKind.Internal,
            parentContext);

        var logger =
            context.CreateReplaySafeLogger<CreateAuthorOrchestrator>();

        // Set orchestration tags
        orchestrationActivity?.SetTag("orchestration.instance_id", context.InstanceId);
        orchestrationActivity?.SetTag("orchestration.name", nameof(CreateAuthorOrchestrator));
        orchestrationActivity?.SetTag("orchestration.is_replaying", context.IsReplaying);

        if (context.IsReplaying)
        {
            // During replay, just log but don't create additional spans
            logger.LogDebug(
                "Replaying orchestration {InstanceId}",
                context.InstanceId);

            orchestrationActivity?.SetTag("orchestration.replaying", true);
            // Note: Don't set status here as it will be overwritten
        }
        else
        {
            logger.LogInformation(
                "Starting CreateAuthor orchestration. InstanceId: {InstanceId}.",
                context.InstanceId);
        }

        logger.LogInformation(
            "Starting CreateAuthor orchestration. InstanceId: {InstanceId}.",
            context.InstanceId);

        var courseEvent =
            context.GetInput<AuthorCreatedEvent>();

        if (courseEvent is null)
        {
            orchestrationActivity?.SetStatus(ActivityStatusCode.Error, "Null event input");

            logger.LogError(
                "CreateAuthor orchestration {InstanceId} did not receive an AuthorCreatedEvent.",
                context.InstanceId);

            throw new InvalidOperationException(
                "AuthorCreatedEvent was not provided.");
        }

        // Set event details (always set these, even during replay)
        orchestrationActivity?.SetTag("event.author_id", courseEvent.AuthorId);
        orchestrationActivity?.SetTag("event.type", courseEvent.GetType().Name);

        if (!context.IsReplaying)
        {
            logger.LogInformation(
            "Processing AuthorCreatedEvent for AuthorId {AuthorId}.",
            courseEvent.AuthorId);
        }

        await context.CallActivityAsync(
            nameof(CreateAuthorAuditActivity),
            courseEvent);

        // Set final status
        orchestrationActivity?.SetStatus(ActivityStatusCode.Ok);

        if (!context.IsReplaying)
        {
            logger.LogInformation(
                "CreateAuthor orchestration {InstanceId} completed for AuthorId {AuthorId}.",
                context.InstanceId,
                courseEvent.AuthorId);
        }
    }
}