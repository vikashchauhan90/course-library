namespace CourseLibrary.Platform.Persistence.Sql.Observability;

public static class SqlMetricConstants
{
    public const string MeterName = SqlTelemetry.MeterName;
    public const string CommandsInstrumentName = "course_library.sql.commands";
    public const string FailuresInstrumentName = "course_library.sql.command.failures";
    public const string DurationInstrumentName = "course_library.sql.command.duration";
}
