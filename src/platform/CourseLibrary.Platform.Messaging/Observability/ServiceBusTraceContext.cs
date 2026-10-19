using Azure.Messaging.ServiceBus;
using CourseLibrary.Platform.Messaging.Abstractions;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;

namespace CourseLibrary.Platform.Messaging.Observability;

public static class ServiceBusTraceContext
{
    public const string DiagnosticId = "Diagnostic-Id";
    public const string TraceParent = "traceparent";
    public const string TraceState = "tracestate";
    public const string EventId = "Id";
    public const string EventType = "Type";
    public const string EventOccurredAt = "OccurredAt";
    public const string Destination = "Destination";
    public const string UserId = "UserId";
    public const string Source = "source";
    public const string TenantId = "Tenant-Id";

    public static readonly TextMapPropagator Propagator =
    Propagators.DefaultTextMapPropagator;

    public static void Inject(
       ServiceBusMessage message,
       Activity? activity = null)
    {
        ArgumentNullException.ThrowIfNull(message);

        activity ??= Activity.Current;

        if (activity is null)
        {
            return;
        }

        Inject(message, new PropagationContext(activity.Context, Baggage.Current));
    }

    public static void Inject(
        ServiceBusMessage message,
        ActivityContext context)
    {
        ArgumentNullException.ThrowIfNull(message);

        Inject(message, new PropagationContext(context, Baggage.Current));
    }

    public static void Inject(
        ServiceBusMessage message,
        ActivityContext context,
        EventMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(metadata);

        var baggage = Baggage.Current.GetBaggage()
            .ToDictionary(static item => item.Key, static item => item.Value);
        foreach (var item in metadata.Baggage)
        {
            baggage[item.Key] = item.Value;
        }

        Inject(
            message,
            new PropagationContext(context, Baggage.Create(baggage)));
    }

    private static void Inject(
        ServiceBusMessage message,
        PropagationContext context)
    {
        var carrier = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);

        Propagator.Inject(
            context,
            carrier,
            static (properties, key, value) =>
                properties[key] = value);

        foreach (var kvp in carrier)
        {
            message.ApplicationProperties[kvp.Key] = kvp.Value;
        }

        if (carrier.TryGetValue(
                TraceParent,
                out var traceParent))
        {
            message.ApplicationProperties[DiagnosticId] = traceParent;
        }
    }

    public static PropagationContext Extract(
        ServiceBusReceivedMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var carrier = CreateCarrier(message);

        return Propagator.Extract(
            default,
            carrier,
            static (properties, key) =>
                properties.TryGetValue(key, out var value)
                    ? [value]
                    : []);
    }

    public static EventMetadata CreateMetadata(
        ServiceBusReceivedMessage message,
        PropagationContext propagationContext)
    {
        ArgumentNullException.ThrowIfNull(message);

        var properties = ImmutableDictionary.CreateBuilder<string, string>(
            StringComparer.OrdinalIgnoreCase);
        foreach (var property in message.ApplicationProperties)
        {
            if (property.Key.Equals(TraceParent, StringComparison.OrdinalIgnoreCase)
                || property.Key.Equals(TraceState, StringComparison.OrdinalIgnoreCase)
                || property.Key.Equals(DiagnosticId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var value = ToMetadataValue(property.Value);
            if (value is not null)
            {
                properties[property.Key] = value;
            }
        }

        if (message.CorrelationId is not null)
        {
            properties["CorrelationId"] = message.CorrelationId;
        }

        var baggage = ImmutableDictionary.CreateBuilder<string, string>(
            StringComparer.Ordinal);
        foreach (var item in propagationContext.Baggage)
        {
            if (item.Value is not null)
            {
                baggage[item.Key] = item.Value;
            }
        }

        var traceParent = GetStringProperty(message, TraceParent)
            ?? GetStringProperty(message, DiagnosticId);
        var traceState = GetStringProperty(message, TraceState);

        return new EventMetadata
        {
            TraceParent = traceParent
                ?? CreateTraceParent(propagationContext.ActivityContext),
            TraceState = traceState ?? propagationContext.ActivityContext.TraceState,
            Properties = properties.ToImmutable(),
            Baggage = baggage.ToImmutable()
        };
    }

    private static Dictionary<string, string> CreateCarrier(
        ServiceBusReceivedMessage message)
    {
        var carrier = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var property in message.ApplicationProperties)
        {
            if (property.Value is string value)
            {
                carrier[property.Key] = value;
            }
        }

        return carrier;
    }

    private static string? GetStringProperty(
        ServiceBusReceivedMessage message,
        string name)
        => message.ApplicationProperties.TryGetValue(name, out var value)
            ? value as string
            : null;

    private static string? ToMetadataValue(object? value)
        => value switch
        {
            null => null,
            string text => text,
            DateTime dateTime => dateTime.ToString("O", CultureInfo.InvariantCulture),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("O", CultureInfo.InvariantCulture),
            Guid guid => guid.ToString("D"),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => null
        };

    private static string? CreateTraceParent(ActivityContext context)
        => context.TraceId == default
            ? null
            : $"00-{context.TraceId}-{context.SpanId}-{(byte)context.TraceFlags:x2}";
}
