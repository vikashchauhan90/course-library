using CourseLibrary.Idp.Infrastructure.Observability.Exceptions;
using OpenTelemetry;
using OpenTelemetry.Logs;

namespace CourseLibrary.Idp.Infrastructure.Observability.Logs.Processors;

public sealed class CourseLibraryLogProcessor(
    string propertyPrefix = "course.library")
    : BaseProcessor<LogRecord>
{
    public override void OnEnd(LogRecord logRecord)
    {
        var attributeCount = logRecord.Attributes?.Count ?? 0;
        var exceptionDataCount = logRecord.Exception?.Data.Count ?? 0;
        if (attributeCount == 0 && exceptionDataCount == 0)
        {
            return;
        }

        var updated = new List<KeyValuePair<string, object?>>(attributeCount + exceptionDataCount);

        foreach (var attribute in LogAttributeHelper.GetAttributes(logRecord))
        {
            updated.Add(
                new KeyValuePair<string, object?>(
                    NormalizeKey(attribute.Key),
                    attribute.Value));
        }

        foreach (var entry in ExceptionDataHelper.GetEntries(logRecord.Exception))
        {
            updated.Add(
                new KeyValuePair<string, object?>(
                    NormalizeKey(entry.Key),
                    entry.Value));
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

        return $"{propertyPrefix}.{key}";
    }
}