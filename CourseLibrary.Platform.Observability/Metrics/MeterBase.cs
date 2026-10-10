using System.Diagnostics.Metrics;

namespace CourseLibrary.Platform.Observability;

public abstract class MeterBase : IDisposable
{
    private readonly Lazy<Meter> _meter;

    protected MeterBase(string meterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(meterName);

        MeterName = meterName;

        _meter = new(
            () => new Meter(MeterName),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    protected string MeterName { get; }

    protected Meter Meter => _meter.Value;

    protected Counter<T> CreateCounter<T>(
        string name,
        string? unit = null,
        string? description = null)
        where T : struct
        => Meter.CreateCounter<T>(
            name,
            unit,
            description);

    protected Histogram<T> CreateHistogram<T>(
        string name,
        string? unit = null,
        string? description = null)
        where T : struct
        => Meter.CreateHistogram<T>(
            name,
            unit,
            description);

    protected UpDownCounter<T> CreateUpDownCounter<T>(
        string name,
        string? unit = null,
        string? description = null)
        where T : struct
        => Meter.CreateUpDownCounter<T>(
            name,
            unit,
            description);

    protected ObservableCounter<T> CreateObservableCounter<T>(
        string name,
        Func<T> observeValue,
        string? unit = null,
        string? description = null)
        where T : struct
        => Meter.CreateObservableCounter(
            name,
            observeValue,
            unit,
            description);

    protected ObservableGauge<T> CreateObservableGauge<T>(
        string name,
        Func<T> observeValue,
        string? unit = null,
        string? description = null)
        where T : struct
        => Meter.CreateObservableGauge(
            name,
            observeValue,
            unit,
            description);

    public void Dispose()
    {
        if (_meter.IsValueCreated)
        {
            _meter.Value.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}