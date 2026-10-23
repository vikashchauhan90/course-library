using OpenTelemetry;
using OpenTelemetry.Logs;

namespace CourseLibrary.Platform.Observability.Logs.Processors;

public sealed class CourseLibraryLogProcessor(
    IPrefixProvider? prefixProvider)
    : BaseProcessor<LogRecord>
{
    private const string propertyPrefix = "course.library.platform";
    public override void OnEnd(LogRecord logRecord)
    {
        var attributeCount = logRecord.Attributes?.Count ?? 0;
        if (attributeCount == 0)
        {
            return;
        }

        var updated = new List<KeyValuePair<string, object?>>(attributeCount);

        foreach (var attribute in LogAttributeHelper.GetAttributes(logRecord))
        {
            updated.Add(
                new KeyValuePair<string, object?>(
                    NormalizeKey(attribute.Key),
                    attribute.Value));
        }

        logRecord.Attributes = updated;
    }

    private string NormalizeKey(string key)
    {
        // Preserve OpenTelemetry semantic attributes.
        if (key.Contains('.'))
        {
            return key;
        }

        if (key == "{OriginalFormat}")
        {
            return key;
        }

        return $"{prefixProvider?.Prefix ?? propertyPrefix}.{key}";
    }
}