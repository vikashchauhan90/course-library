using OpenTelemetry;
using OpenTelemetry.Logs;

namespace CourseLibrary.Platform.Observability.Logs.Processors;

public sealed class CourseLibraryLogProcessor
    : BaseProcessor<LogRecord>
{
    private const string DefaultPrefix = "course.library.platform";
    private const char PrefixSeparator = '.';
    private const string OriginalFormatKey = "{OriginalFormat}";

    private readonly string _prefixWithSeparator;

    public CourseLibraryLogProcessor(IPrefixProvider? prefixProvider)
    {
        var supplied = prefixProvider?.Prefix;
        var prefix = string.IsNullOrWhiteSpace(supplied) ? DefaultPrefix : supplied;
        _prefixWithSeparator = prefix + PrefixSeparator;
    }

    public override void OnEnd(LogRecord logRecord)
    {
        ArgumentNullException.ThrowIfNull(logRecord);

        var attributes = logRecord.Attributes;
        if (attributes is null || attributes.Count == 0)
        {
            return;
        }

        if (!AnyNeedsPrefixing(attributes))
        {
            return;
        }

        var updated = new List<KeyValuePair<string, object?>>(attributes.Count);
        foreach (var attribute in attributes)
        {
            var key = NeedsPrefix(attribute.Key)
                ? _prefixWithSeparator + attribute.Key
                : attribute.Key;
            updated.Add(new KeyValuePair<string, object?>(key, attribute.Value));
        }

        logRecord.Attributes = updated;
    }

    private bool AnyNeedsPrefixing(IReadOnlyList<KeyValuePair<string, object?>> attributes)
    {
        for (var i = 0; i < attributes.Count; i++)
        {
            if (NeedsPrefix(attributes[i].Key))
            {
                return true;
            }
        }
        return false;
    }

    private bool NeedsPrefix(string key)
    {
        if (IsOriginalFormat(key))
        {
            return false;
        }

        // Already prefixed (idempotency).
        if (key.StartsWith(_prefixWithSeparator, StringComparison.Ordinal))
        {
            return false;
        }

        // Preserve OTel semantic conventions and any other dotted key.
        if (key.IndexOf(PrefixSeparator) > 0)
        {
            return false;
        }

        return true;
    }

    private static bool IsOriginalFormat(string key)
        => string.Equals(key, OriginalFormatKey, StringComparison.Ordinal);
}