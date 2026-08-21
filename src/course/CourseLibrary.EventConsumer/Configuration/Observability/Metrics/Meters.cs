using System.Diagnostics.Metrics;

namespace CourseLibrary.EventConsumer.Configuration.Observability.Metrics;

public static class Meters
{
    public const string Name =
       "CourseLibrary.EventConsumer";

    public static readonly Meter EventConsumer =
        new(Name);

    public static readonly Counter<long> FunctionCount =
        EventConsumer.CreateCounter<long>(
            "function.execution.count");

    public static readonly Histogram<double> FunctionDuration =
        EventConsumer.CreateHistogram<double>(
            "function.execution.duration",
            "ms");

    public static readonly UpDownCounter<long> ActiveFunctions =
        EventConsumer.CreateUpDownCounter<long>(
            "function.execution.active");
}
