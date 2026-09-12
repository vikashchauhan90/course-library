using Azure.Messaging.ServiceBus;
using CourseLibrary.Application.Abstractions.Messaging;
using CourseLibrary.Application.Abstractions.Serialization;
using CourseLibrary.Application.Abstractions.Serializers;
using CourseLibrary.Domain.Events;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;


namespace CourseLibrary.EventConsumer.Consumers.Courses;

internal sealed class CourseEventDqlMessageHandler(
    ISerializerFactory serializerFactory,
    IDqlMessageStore dqlMessageStore,
    ILogger<CourseEventDqlMessageHandler> logger)
{
    private readonly ISerializer<CourseEvent> serializer =
        serializerFactory.Create<CourseEvent>(
            SerializerType.MessagePack);

    [Function(nameof(CourseEventDqlMessageHandler))]
    public async Task RunAsync(
        [ServiceBusTrigger(
            "CourseEvent/$DeadLetterQueue",
            "CourseEventDqlMessageHandler",
            Connection = "ServiceBusConnection",
            AutoCompleteMessages = false)]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        try
        {
            CourseEvent? courseEvent = null;

            try
            {
                courseEvent = serializer.Deserialize(
                    message.Body.ToArray());
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    exception,
                    "Unable to deserialize dead-lettered CourseEvent {MessageId}.",
                    message.MessageId);
            }

            var record = new DqlMessage
            {
                Id = message.MessageId,
                EventType = courseEvent?.EventType.ToString()
                    ?? "Unknown",

                MessageId = message.MessageId,
                CorrelationId = message.CorrelationId,
                Subject = message.Subject,

                DeadLetterReason =
                    message.DeadLetterReason,

                DeadLetterErrorDescription =
                    message.DeadLetterErrorDescription,

                EnqueuedTime = message.EnqueuedTime,
                DeadLetteredAt = DateTimeOffset.UtcNow,

                DeliveryCount = message.DeliveryCount,

                Payload = Convert.ToBase64String(
                    message.Body.ToArray())
            };

            await dqlMessageStore.StoreAsync(
                record,
                cancellationToken);

            await messageActions.CompleteMessageAsync(
                message,
                cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Failed to process DQL message {MessageId}.",
                message.MessageId);

            throw;
        }
    }
}