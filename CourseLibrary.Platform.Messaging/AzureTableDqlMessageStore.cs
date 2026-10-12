using Azure.Data.Tables;
using CourseLibrary.Platform.Messaging.Abstractions;
using CourseLibrary.Platform.Messaging.Configurations;
using CourseLibrary.Platform.Messaging.Observability;
using CourseLibrary.Platform.Observability.Traces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.Platform.Messaging;

internal sealed class AzureTableDqlMessageStore(
     [FromKeyedServices(AzureStorageOptions.KeyedServiceName)] TableClient tableClient,
     ILogger<AzureTableDqlMessageStore> logger)
    : IDqlMessageStore
{
    private static readonly PlatformActivitySource ActivitySources = new();
    private static readonly MessagingMeter Metrics = MessagingMeter.Shared;

    public async Task StoreAsync(
        DqlMessage message,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentException.ThrowIfNullOrWhiteSpace(message.EventType);
        ArgumentException.ThrowIfNullOrWhiteSpace(message.MessageId);

        var startedAt = Stopwatch.GetTimestamp();
        var outcome = "error";
        using var activity = ActivitySources.StartActivity(
            "AzureTableDqlMessageStore.StoreAsync",
            ActivityKind.Client);

        activity?.SetTag(
            MessagingActivityTagConstants.Operation,
            MessagingActivityTagConstants.StoreDeadLetterOperation);
        activity?.SetTag(MessagingActivityTagConstants.System, "azure_table");
        activity?.SetTag(MessagingActivityTagConstants.Destination, tableClient.Name);
        activity?.SetTag(MessagingActivityTagConstants.EventType, message.EventType);
        activity?.SetTag(MessagingActivityTagConstants.MessageId, message.MessageId);
        activity?.SetTag(MessagingActivityTagConstants.DeliveryCount, message.DeliveryCount);
        activity?.SetTag(MessagingActivityTagConstants.Status, message.Status.ToString());

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var entity = new TableEntity(
                partitionKey: message.EventType,
                rowKey: message.MessageId)
            {
                ["CorrelationId"] = message.CorrelationId,
                ["TraceParent"] = message.TraceParent,
                ["TraceState"] = message.TraceState,
                ["Subject"] = message.Subject,
                ["DeadLetterReason"] = message.DeadLetterReason,
                ["DeadLetterErrorDescription"] =
                    message.DeadLetterErrorDescription,
                ["EnqueuedTime"] = message.EnqueuedTime,
                ["DeadLetteredAt"] = message.DeadLetteredAt,
                ["DeliveryCount"] = message.DeliveryCount,
                ["Status"] = message.Status.ToString(),
                ["ReplayedAt"] = message.ReplayedAt,
                ["ReplayedMessageId"] = message.ReplayedMessageId,
                ["LastError"] = message.LastError,
                ["Payload"] = message.Payload
            };

            await tableClient.UpsertEntityAsync(
                entity,
                TableUpdateMode.Replace,
                cancellationToken);

            outcome = "success";
            activity?.SetStatus(ActivityStatusCode.Ok);
            logger.DeadLetterStored(message.MessageId, message.Status.ToString());
        }
        catch (OperationCanceledException)
        {
            outcome = "cancelled";
            activity?.SetTag(MessagingActivityTagConstants.Cancelled, true);
            logger.DeadLetterStoreCancelled(message.MessageId);
            throw;
        }
        catch (Exception exception)
        {
            activity?.SetTag(
                MessagingActivityTagConstants.ErrorType,
                exception.GetType().FullName);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Dead-letter storage operation failed.");
            logger.DeadLetterStoreFailed(exception, message.MessageId);
            throw;
        }
        finally
        {
            activity?.SetTag(MessagingActivityTagConstants.Outcome, outcome);
            Metrics.RecordOperation(
                MessagingActivityTagConstants.StoreDeadLetterOperation,
                outcome,
                Stopwatch.GetElapsedTime(startedAt).TotalSeconds);
        }
    }
}
