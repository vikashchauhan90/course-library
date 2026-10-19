using System.Diagnostics.Metrics;

namespace CourseLibrary.Platform.Observability;

public abstract class MeterBase
{
    private readonly Meter _meter;

    protected MeterBase(string meterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(meterName);

        _meter = new Meter(meterName);
    }

    protected Meter Meter => _meter;

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
}