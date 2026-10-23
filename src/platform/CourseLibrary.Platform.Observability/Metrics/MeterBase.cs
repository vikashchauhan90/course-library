using System.Diagnostics.Metrics;

namespace CourseLibrary.Platform.Observability;

public abstract class MeterBase: IDisposable
{
    private readonly Meter _meter;
    private int _disposed;
    protected MeterBase(string meterName, string? version ="")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(meterName);

        _meter = new Meter(meterName, version ?? string.Empty);
    }

    protected Meter Meter => _meter;
    public string Name => _meter.Name;

    protected Counter<T> CreateCounter<T>(
        string name,
        string? unit = null,
        string? description = null)
        where T : struct
    {
        ValidateInstrumentName(name);

        return Meter.CreateCounter<T>(
            name,
            unit,
            description);
    }

    protected Histogram<T> CreateHistogram<T>(
        string name,
        string? unit = null,
        string? description = null)
        where T : struct
    {
        ValidateInstrumentName(name);

        return Meter.CreateHistogram<T>(
            name,
            unit,
            description);
    }

    protected UpDownCounter<T> CreateUpDownCounter<T>(
        string name,
        string? unit = null,
        string? description = null)
        where T : struct
    {
        ValidateInstrumentName(name);

        return Meter.CreateUpDownCounter<T>(
            name,
            unit,
            description);
    }

    protected ObservableCounter<T> CreateObservableCounter<T>(
        string name,
        Func<T> observeValue,
        string? unit = null,
        string? description = null)
        where T : struct
    {
        ArgumentNullException.ThrowIfNull(observeValue);
        ValidateInstrumentName(name);

        return Meter.CreateObservableCounter(
            name,
            observeValue,
            unit,
            description);
    }

    protected ObservableGauge<T> CreateObservableGauge<T>(
        string name,
        Func<T> observeValue,
        string? unit = null,
        string? description = null)
        where T : struct
    {
        ArgumentNullException.ThrowIfNull(observeValue);
        ValidateInstrumentName(name);

        return Meter.CreateObservableGauge(
            name,
            observeValue,
            unit,
            description);
    }

    protected ObservableUpDownCounter<T> CreateObservableUpDownCounter<T>(
        string name,
        Func<T> observeValue,
        string? unit = null,
        string? description = null)
        where T : struct
    {
        ArgumentNullException.ThrowIfNull(observeValue);
        ValidateInstrumentName(name);

        return Meter.CreateObservableUpDownCounter(
            name,
            observeValue,
            unit,
            description);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
        {
            return;
        }

        if (disposing)
        {
            _meter.Dispose();
        }
    }

    private static void ValidateInstrumentName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
    }
}