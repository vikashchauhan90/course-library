using OpenTelemetry.Logs;

namespace CourseLibrary.Idp.Infrastructure.Observability.Logs;

public static class LogAttributeHelper
{
    public static IEnumerable<KeyValuePair<string, object?>> GetAttributes(
        LogRecord? logRecord)
    {
        if (logRecord?.Attributes is null ||
            logRecord.Attributes.Count == 0)
        {
            yield break;
        }

        foreach (var attribute in logRecord.Attributes)
        {
            if (string.IsNullOrWhiteSpace(attribute.Key))
            {
                continue;
            }

            yield return new KeyValuePair<string, object?>(
                attribute.Key,
                attribute.Value);
        }
    }
}
