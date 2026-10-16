using Microsoft.Extensions.Logging;

namespace CourseLibrary.Platform.Serialization.Observability;

internal static partial class SerializationLogs
{
    [LoggerMessage(
        5201,
        LogLevel.Debug,
        "Serialization completed using {Serializer} for {TypeName}.",
        EventName = nameof(SerializationSucceeded))]
    public static partial void SerializationSucceeded(
        this ILogger logger,
        string serializer,
        string typeName);

    [LoggerMessage(
        5202,
        LogLevel.Error,
        "Serialization failed using {Serializer} for {TypeName}.",
        EventName = nameof(SerializationFailed))]
    public static partial void SerializationFailed(
        this ILogger logger,
        Exception exception,
        string serializer,
        string typeName);

    [LoggerMessage(
        5203,
        LogLevel.Debug,
        "Deserialization completed using {Serializer} for {TypeName}.",
        EventName = nameof(DeserializationSucceeded))]
    public static partial void DeserializationSucceeded(
        this ILogger logger,
        string serializer,
        string typeName);

    [LoggerMessage(
        5204,
        LogLevel.Error,
        "Deserialization failed using {Serializer} for {TypeName}.",
        EventName = nameof(DeserializationFailed))]
    public static partial void DeserializationFailed(
        this ILogger logger,
        Exception exception,
        string serializer,
        string typeName);
}
