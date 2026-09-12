using CourseLibrary.Application.Abstractions.Messaging;
using CourseLibrary.Domain.Events;
using Azure.Data.Tables;


namespace CourseLibrary.Infrastructure.Messaging.AzureTable;

internal sealed class AzureTableDqlMessageStore(
    TableClient tableClient)
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
            ["Status"] = "DeadLettered"
        };

        await tableClient.UpsertEntityAsync(
            entity,
            TableUpdateMode.Replace,
            cancellationToken);
    }
}