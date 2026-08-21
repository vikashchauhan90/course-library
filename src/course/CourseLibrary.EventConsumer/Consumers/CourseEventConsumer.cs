using Azure.Messaging.ServiceBus;
using CourseLibrary.Domain.Events;
using CourseLibrary.EventConsumer.Configuration.Observability.Traces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.EventConsumer.Consumers;

public sealed class CourseEventConsumer(
    ILogger<CourseEventConsumer> logger)
{
    [Function("CourseEventConsumer")]
    public async Task RunAsync(
        [ServiceBusTrigger(
            "%CourseEventsQueue%",
            Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message)
    {
        var parentContext =
     ServiceBusTraceContext.Extract(message);

        using var activity =
            ActivitySources.EventConsumer.StartActivity(
                "course.event.process",
                ActivityKind.Consumer,
                parentContext);

        logger.LogInformation(
            "Processing Service Bus message {MessageId}.",
            message.MessageId);


        var courseEvent =
            message.Body.ToObjectFromJson<CourseCreatedEvent>();

        // Deserialize event
        // Save to Cosmos
        await Task.CompletedTask;
    }
}