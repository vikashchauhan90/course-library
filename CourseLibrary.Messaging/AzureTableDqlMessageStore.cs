using Azure.Data.Tables;
using CourseLibrary.Messaging.Abstractions;
using CourseLibrary.Messaging.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace CourseLibrary.Messaging;

internal sealed class AzureTableDqlMessageStore(
     [FromKeyedServices(AzureStorageOptions.KeyedServiceName)] TableClient tableClient)
    : IDqlMessageStore
{
    public async Task StoreAsync(
        DqlMessage message,
        CancellationToken cancellationToken)
    {
        var entity = new TableEntity(
            partitionKey: message.EventType,
            rowKey: message.MessageId)
        {
            ["CorrelationId"] = message.CorrelationId,
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
    }
}
