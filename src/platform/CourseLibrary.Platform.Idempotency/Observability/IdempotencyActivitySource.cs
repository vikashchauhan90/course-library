using CourseLibrary.Platform.Observability.Traces;

namespace CourseLibrary.Platform.Idempotency.Observability;

internal sealed class IdempotencyActivitySource : ActivitySourceBase
{
    private static readonly Lazy<IdempotencyActivitySource> SharedInstance = new(
        static () => new IdempotencyActivitySource(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static IdempotencyActivitySource Shared => SharedInstance.Value;

    private IdempotencyActivitySource()
        : base(IdempotencyTelemetry.ActivitySourceName)
    {
    }
}
