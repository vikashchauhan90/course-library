using CourseLibrary.Platform.Observability.Traces;

namespace CourseLibrary.Platform.Serialization.Observability;

internal sealed class SerializationActivitySource : ActivitySourceBase
{
    private static readonly Lazy<SerializationActivitySource> SharedInstance = new(
        static () => new SerializationActivitySource(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static SerializationActivitySource Shared => SharedInstance.Value;

    private SerializationActivitySource()
        : base(SerializationTelemetry.ActivitySourceName)
    {
    }
}
