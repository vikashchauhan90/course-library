using CourseLibrary.Platform.DataProtection.Observability;
using CourseLibrary.Platform.Observability.Traces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.Platform.DataProtection;

internal sealed class DataProtectionService(
    IDataProtectionProvider dataProtectionProvider,
    ILogger<DataProtectionService> logger)
    : IDataProtectionService
{
    private const string Purpose = "CourseLibrary.DataProtection";
    private static readonly PlatformActivitySource ActivitySources = new();
    private static readonly DataProtectionMeter Metrics = DataProtectionMeter.Shared;

    private readonly IDataProtector _protector =
        dataProtectionProvider.CreateProtector(Purpose);

    public string Protect(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Execute(
            DataProtectionActivityTagConstants.ProtectOperation,
            DataProtectionActivityTagConstants.StringPayload,
            () => _protector.Protect(value));
    }

    public string Unprotect(string protectedValue)
    {
        ArgumentNullException.ThrowIfNull(protectedValue);

        return Execute(
            DataProtectionActivityTagConstants.UnprotectOperation,
            DataProtectionActivityTagConstants.StringPayload,
            () => _protector.Unprotect(protectedValue));
    }

    public byte[] Protect(byte[] value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Execute(
            DataProtectionActivityTagConstants.ProtectOperation,
            DataProtectionActivityTagConstants.ByteArrayPayload,
            () => _protector.Protect(value));
    }

    public byte[] Unprotect(byte[] protectedValue)
    {
        ArgumentNullException.ThrowIfNull(protectedValue);

        return Execute(
            DataProtectionActivityTagConstants.UnprotectOperation,
            DataProtectionActivityTagConstants.ByteArrayPayload,
            () => _protector.Unprotect(protectedValue));
    }

    private T Execute<T>(string operation, string payloadType, Func<T> action)
    {
        using var activity = ActivitySources.StartActivity(
            $"DataProtectionService.{operation}");

        activity?.SetTag(DataProtectionActivityTagConstants.Operation, operation);
        activity?.SetTag(DataProtectionActivityTagConstants.PayloadType, payloadType);

        try
        {
            var result = action();
            Metrics.RecordOperation(operation, payloadType, "success");
            activity?.SetTag(DataProtectionActivityTagConstants.OperationSuccess, true);
            logger.OperationSucceeded(operation, payloadType);
            return result;
        }
        catch (Exception exception)
        {
            Metrics.RecordOperation(operation, payloadType, "error");
            activity?.SetTag(DataProtectionActivityTagConstants.OperationError, true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Data protection operation failed.");

            logger.OperationFailed(exception, operation, payloadType);
            throw;
        }
    }
}