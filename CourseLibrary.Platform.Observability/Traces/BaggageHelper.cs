using OpenTelemetry;

namespace CourseLibrary.Platform.Observability.Traces;


public static class BaggageHelper
{
    public static void Set(
        string key,
        string value)
    {
        Baggage.SetBaggage(
            key,
            value);
    }

    public static string? Get(
        string key)
    {
        return Baggage.GetBaggage(key);
    }

    public static IReadOnlyDictionary<string, string>
        GetAll() =>
        Baggage.Current.GetBaggage();

    public static void Clear() =>
        Baggage.Current.ClearBaggage();
}