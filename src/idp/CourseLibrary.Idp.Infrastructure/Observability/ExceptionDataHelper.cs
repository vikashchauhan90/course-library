using System.Collections;

namespace CourseLibrary.Idp.Infrastructure.Observability.Exceptions;

public static class ExceptionDataHelper
{
    public static IEnumerable<KeyValuePair<string, object?>> GetEntries(
        Exception? exception)
    {
        if (exception is null ||
            exception.Data.Count == 0)
        {
            yield break;
        }

        foreach (DictionaryEntry entry in exception.Data)
        {
            var key = entry.Key?.ToString();

            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            yield return new KeyValuePair<string, object?>(
                $"exception.data.{key}",
                entry.Value);
        }
    }
}