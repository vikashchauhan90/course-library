using Azure.Messaging.ServiceBus;
using CourseLibrary.Platform.Messaging.Abstractions;
using CourseLibrary.Platform.Messaging.Observability;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.Platform.Messaging;

internal sealed class ServiceBusEventPublisher(
    ServiceBusClient client,
    ILogger<ServiceBusEventPublisher> logger)
    : IEventPublisher
{
    private const string ActivityName = "ServiceBusEventPublisher.PublishAsync";
    private const int RecommendedStandardTierMessageSize = 256 * 1024;

    private static readonly MessagingActivitySource ActivitySource = MessagingActivitySource.Shared;
    private static readonly MessagingMeter Metrics = MessagingMeter.Shared;

    public async Task PublishAsync(
        string queueOrTopicName,
        ServiceBusEventPayload @event,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
               "Current trace context: TraceId={TraceId}, SpanId={SpanId}, ParentSpanId={ParentSpanId}",
               Activity.Current?.TraceId.ToString(),
               Activity.Current?.SpanId.ToString(),
               Activity.Current?.ParentSpanId.ToString());

        ArgumentException.ThrowIfNullOrWhiteSpace(queueOrTopicName);
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(@event.Payload);
        ArgumentNullException.ThrowIfNull(@event.Metadata);
        ArgumentNullException.ThrowIfNull(@event.Metadata.Properties);

        var hasTraceMetadata =
            !string.IsNullOrWhiteSpace(@event.Metadata.TraceParent)
            || !string.IsNullOrWhiteSpace(@event.Metadata.TraceState);
        var hasParentContext = ActivityContext.TryParse(
            @event.Metadata.TraceParent,
            @event.Metadata.TraceState,
            out var parentContext);

        Activity? activity;
        if (hasParentContext)
        {
            activity = ActivitySource.StartActivity(
                ActivityName,
                ActivityKind.Producer,
                parentContext);
        }
        else
        {
            if (hasTraceMetadata)
            {
                logger.InvalidTraceContext(@event.EventId);
            }

            activity = ActivitySource.StartActivity(
                ActivityName,
                ActivityKind.Producer);
        }

        using (activity)
        {
            var startedAt = Stopwatch.GetTimestamp();
            var outcome = "error";
            var payloadSize = @event.Payload.Length;
            Metrics.RecordMessageSize(payloadSize);

            var userId = @event.Metadata.Properties.TryGetValue(
                "userId",
                out var userIdValue)
                ? userIdValue
                : null;
            var correlationId = @event.Metadata.Properties.TryGetValue(
                "correlationId",
                out var correlationIdValue)
                ? correlationIdValue
                : @event.CorrelationId?.ToString();
            var tenantId = @event.Metadata.Properties.TryGetValue(
                "tenantId",
                out var tenantIdValue)
                ? tenantIdValue
                : null;

            activity?.SetTag(MessagingActivityTagConstants.System, "azure_service_bus");
            activity?.SetTag(MessagingActivityTagConstants.Operation, MessagingActivityTagConstants.PublishOperation);
            activity?.SetTag(MessagingActivityTagConstants.Destination, queueOrTopicName);
            activity?.SetTag(MessagingActivityTagConstants.EventType, @event.EventType);
            activity?.SetTag(MessagingActivityTagConstants.MessageId, @event.EventId.ToString());
            activity?.SetTag(MessagingActivityTagConstants.ContentType, @event.Payload.MediaType);
            activity?.SetTag(MessagingActivityTagConstants.MessageSize, payloadSize);
            activity?.SetTag("request.correlationId", correlationId);

            try
            {
                var message = new ServiceBusMessage(@event.Payload)
                {
                    MessageId = @event.EventId.ToString(),
                    Subject = @event.Subject ?? @event.EventType,
                    ContentType = @event.Payload.MediaType,
                    CorrelationId = correlationId,
                    ApplicationProperties =
                    {
                        [ServiceBusTraceContext.EventId] = @event.EventId.ToString(),
                        [ServiceBusTraceContext.EventOccurredAt] = @event.OccurredAt.ToUnixTimeMilliseconds(),
                        [ServiceBusTraceContext.EventType] = @event.EventType,
                        [ServiceBusTraceContext.Destination] = queueOrTopicName,
                        [ServiceBusTraceContext.UserId] = userId,
                        [ServiceBusTraceContext.Source] = @event.Source ?? nameof(ServiceBusEventPublisher),
                        [ServiceBusTraceContext.TenantId] = tenantId,
                        ["CausationId"] = @event.CausationId?.ToString(),
                        ["Version"] = @event.Version
                    }
                };

                foreach (var property in @event.Metadata.Properties)
                {
                    var isReservedProperty = message.ApplicationProperties.Keys.Any(
                        key => key.Equals(
                            property.Key,
                            StringComparison.OrdinalIgnoreCase));
                    if (!isReservedProperty)
                    {
                        message.ApplicationProperties[property.Key] = property.Value;
                    }
                }

                if (activity is not null)
                {
                    ServiceBusTraceContext.Inject(
                        message,
                        activity.Context,
                        @event.Metadata);
                }
                else if (hasParentContext)
                {
                    ServiceBusTraceContext.Inject(
                        message,
                        parentContext,
                        @event.Metadata);
                }
                else if (Activity.Current is { } currentActivity)
                {
                    ServiceBusTraceContext.Inject(
                        message,
                        currentActivity.Context,
                        @event.Metadata);
                }

                if (payloadSize > RecommendedStandardTierMessageSize)
                {
                    logger.MessageSizeExceedsRecommendedLimit(
                        @event.EventId,
                        payloadSize);
                }

                await using var sender = client.CreateSender(queueOrTopicName);
                await sender.SendMessageAsync(message, cancellationToken);

                outcome = "success";
                activity?.SetStatus(ActivityStatusCode.Ok);
                logger.EventPublished(
                    @event.EventType,
                    @event.EventId,
                    queueOrTopicName,
                    Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);
            }
            catch (OperationCanceledException)
            {
                outcome = "cancelled";
                activity?.SetTag(MessagingActivityTagConstants.Cancelled, true);
                logger.PublishCancelled(@event.EventId);
                throw;
            }
            catch (Exception exception)
            {
                activity?.SetTag(
                    MessagingActivityTagConstants.ErrorType,
                    exception.GetType().FullName);
                activity?.SetStatus(
                    ActivityStatusCode.Error,
                    "Messaging publish operation failed.");
                logger.PublishFailed(
                    exception,
                    @event.EventType,
                    @event.EventId);
                throw;
            }
            finally
            {
                activity?.SetTag(MessagingActivityTagConstants.Outcome, outcome);
                Metrics.RecordOperation(
                    MessagingActivityTagConstants.PublishOperation,
                    outcome,
                    Stopwatch.GetElapsedTime(startedAt).TotalSeconds);
            }
        }
    }
}
