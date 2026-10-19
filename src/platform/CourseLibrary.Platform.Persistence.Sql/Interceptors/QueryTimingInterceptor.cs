using CourseLibrary.Platform.Persistence.Sql.Observability;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;

namespace CourseLibrary.Platform.Persistence.Sql.Interceptors;

public sealed class QueryTimingInterceptor(ILogger<QueryTimingInterceptor> logger) : DbCommandInterceptor
{
    private const int WarningThresholdMilliseconds = 500;
    private readonly ConcurrentDictionary<Guid, CommandMeasurement> _measurements = new();

    private void Start(DbCommand command, CommandEventData eventData, string operation)
    {
        var activity = SqlActivitySource.Shared.StartActivity(
            $"db.command.{operation}",
            ActivityKind.Client);
        var databaseSystem = SqlDatabaseSystem.Resolve(command.Connection);
        activity?.SetTag(SqlActivityTagConstants.DbSystem, databaseSystem);
        activity?.SetTag(SqlActivityTagConstants.DbOperationName, operation);
        if (!string.IsNullOrWhiteSpace(command.Connection?.Database))
        {
            activity?.SetTag(SqlActivityTagConstants.DbNamespace, command.Connection.Database);
        }

        _measurements[eventData.CommandId] = new(operation, Stopwatch.StartNew(), activity, databaseSystem);
    }

    private CommandMeasurement? Stop(Guid commandId)
    {
        if (!_measurements.TryRemove(commandId, out var measurement))
        {
            return null;
        }

        measurement.Timer.Stop();
        return measurement;
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        Start(command, eventData, SqlActivityTagConstants.ReaderOperation);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        Start(command, eventData, SqlActivityTagConstants.ReaderOperation);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        Log(eventData, SqlActivityTagConstants.ReaderOperation);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        Log(eventData, SqlActivityTagConstants.ReaderOperation);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result)
    {
        Start(command, eventData, SqlActivityTagConstants.ScalarOperation);
        return base.ScalarExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        Start(command, eventData, SqlActivityTagConstants.ScalarOperation);
        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override object? ScalarExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result)
    {
        Log(eventData, SqlActivityTagConstants.ScalarOperation);
        return base.ScalarExecuted(command, eventData, result);
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        Log(eventData, SqlActivityTagConstants.ScalarOperation);
        return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result)
    {
        Start(command, eventData, SqlActivityTagConstants.NonQueryOperation);
        return base.NonQueryExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Start(command, eventData, SqlActivityTagConstants.NonQueryOperation);
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override int NonQueryExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result)
    {
        Log(eventData, SqlActivityTagConstants.NonQueryOperation);
        return base.NonQueryExecuted(command, eventData, result);
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        Log(eventData, SqlActivityTagConstants.NonQueryOperation);
        return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
    {
        Log(eventData, "command", eventData.Exception);
        base.CommandFailed(command, eventData);
    }

    public override Task CommandFailedAsync(
        DbCommand command,
        CommandErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        Log(eventData, "command", eventData.Exception);
        return base.CommandFailedAsync(command, eventData, cancellationToken);
    }

    public override void CommandCanceled(DbCommand command, CommandEndEventData eventData)
    {
        Log(eventData, "command", failureType: "cancelled");
        base.CommandCanceled(command, eventData);
    }

    public override Task CommandCanceledAsync(
        DbCommand command,
        CommandEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        Log(eventData, "command", failureType: "cancelled");
        return base.CommandCanceledAsync(command, eventData, cancellationToken);
    }

    private void Log(
        CommandEndEventData eventData,
        string fallbackOperation,
        Exception? exception = null,
        string? failureType = null)
    {
        var measurement = Stop(eventData.CommandId);
        var operation = measurement?.Operation ?? fallbackOperation;
        var elapsedMilliseconds = measurement?.Timer.Elapsed.TotalMilliseconds ?? 0;

        if (measurement is not null)
        {
            SqlMeter.Shared.RecordCommand(
                operation,
                elapsedMilliseconds,
                measurement.DatabaseSystem);
            measurement.Activity?.SetTag(SqlActivityTagConstants.DurationMilliseconds, elapsedMilliseconds);
        }

        if (exception is not null)
        {
            failureType = exception.GetType().Name;
            measurement?.Activity?.SetTag(SqlActivityTagConstants.ErrorType, failureType);
            measurement?.Activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            SqlMeter.Shared.RecordFailure(
                operation,
                failureType,
                measurement?.DatabaseSystem ?? SqlActivityTagConstants.Other);
            SqlPersistenceLogs.CommandFailed(logger, exception, operation, elapsedMilliseconds);
        }
        else if (failureType is not null)
        {
            measurement?.Activity?.SetTag(SqlActivityTagConstants.OperationCancelled, true);
            measurement?.Activity?.SetTag(SqlActivityTagConstants.ErrorType, failureType);
            measurement?.Activity?.SetStatus(ActivityStatusCode.Error, "Command was cancelled.");
            SqlMeter.Shared.RecordFailure(
                operation,
                failureType,
                measurement?.DatabaseSystem ?? SqlActivityTagConstants.Other);
            SqlPersistenceLogs.CommandCancelled(logger, operation, elapsedMilliseconds);
        }
        else if (elapsedMilliseconds >= WarningThresholdMilliseconds)
        {
            measurement?.Activity?.SetTag(SqlActivityTagConstants.OperationSuccess, true);
            measurement?.Activity?.SetStatus(ActivityStatusCode.Ok);
            SqlPersistenceLogs.CommandSlow(logger, operation, elapsedMilliseconds);
        }
        else
        {
            measurement?.Activity?.SetTag(SqlActivityTagConstants.OperationSuccess, true);
            measurement?.Activity?.SetStatus(ActivityStatusCode.Ok);
            SqlPersistenceLogs.CommandCompleted(logger, operation, elapsedMilliseconds);
        }

        measurement?.Activity?.Dispose();
    }

    private sealed record CommandMeasurement(
        string Operation,
        Stopwatch Timer,
        Activity? Activity,
        string DatabaseSystem);
}
