using CourseLibrary.Platform.Observability;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CourseLibrary.Platform.Persistence.Sql.Observability;

public sealed class SqlMeter : MeterBase
{
    private static readonly Lazy<SqlMeter> SharedInstance = new(
        static () => new SqlMeter(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static SqlMeter Shared => SharedInstance.Value;

    private readonly Counter<long> _commands;
    private readonly Counter<long> _failures;
    private readonly Histogram<double> _duration;

    private SqlMeter()
        : base(SqlMetricConstants.MeterName)
    {
        _commands = CreateCounter<long>(
            SqlMetricConstants.CommandsInstrumentName,
            "{command}",
            "Number of SQL commands observed.");
        _failures = CreateCounter<long>(
            SqlMetricConstants.FailuresInstrumentName,
            "{failure}",
            "Number of SQL commands that failed or were cancelled.");
        _duration = CreateHistogram<double>(
            SqlMetricConstants.DurationInstrumentName,
            "ms",
            "Duration of SQL commands.");
    }

    public void RecordCommand(string operation, double durationMilliseconds)
    {
        var tags = CreateTags(operation);
        _commands.Add(1, tags);
        _duration.Record(durationMilliseconds, tags);
    }

    public void RecordFailure(string operation, string failureType)
        => _failures.Add(1, CreateTags(operation, failureType));

    private static TagList CreateTags(string operation, string? failureType = null)
    {
        var tags = new TagList
        {
            { SqlActivityTagConstants.DbSystem, SqlActivityTagConstants.SqlServer },
            { SqlActivityTagConstants.DbOperationName, operation }
        };

        if (failureType is not null)
        {
            tags.Add(SqlActivityTagConstants.ErrorType, failureType);
        }

        return tags;
    }
}
