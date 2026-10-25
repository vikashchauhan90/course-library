using CourseLibrary.Course.Contracts.DTOs.Courses.Audits;
using CourseLibrary.Course.Persistence.Core;
using CourseLibrary.Course.Persistence.Cosmos;
using CourseLibrary.Course.Persistence.Observability;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.Course.Persistence.Repositories.Courses.Audits;

internal sealed class CourseAuditRepository : ICourseAuditRepository
{
    private readonly ICosmosContainerFactory _containerFactory;
    private readonly ILogger<CourseAuditRepository> _logger;
    private static readonly CosmosActivitySource ActivitySource = CosmosActivitySource.Shared;
    private static readonly CosmosMeter Meter = CosmosMeter.Shared;
    public CourseAuditRepository(
    ICosmosContainerFactory containerFactory,
    ILogger<CourseAuditRepository> logger)
    {
        _containerFactory = containerFactory;
        _logger = logger;
    }
    public async Task<CourseAuditResponse> AddAsync(
        CourseAuditRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var activity =
        ActivitySource.StartActivity(
            CosmosActivityNames.UpsertItem,
            ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var container = _containerFactory
            .GetContainer(SupportedContainers.CourseAudits);

            activity.SetCosmosOperation(
            OperationNames.UpsertItem,
            SupportedContainers.CourseAudits,
            container.Database.Id);

            var entity = CourseAuditMapper.ToEntity(request);

            ItemRequestOptions requestOptions = new ItemRequestOptions
            {
                EnableContentResponseOnWrite = false,
                ConsistencyLevel = ConsistencyLevel.Eventual
            };

            var response = await container
            .UpsertItemAsync(
                entity,
                partitionKey: new PartitionKey(entity.PartitionKey),
                requestOptions: requestOptions,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

            RecordSuccess(activity, OperationNames.UpsertItem, startedAt, response.RequestCharge);

            return CourseAuditMapper.ToResponse(response.Resource);

        }
        catch (CosmosException ex)
        {
            RecordFailure(activity, OperationNames.UpsertItem, startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            RecordFailure(activity, OperationNames.UpsertItem, startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            RecordFailure(activity, OperationNames.UpsertItem, startedAt, ex);
            throw;
        }

    }

    private void RecordSuccess(
        Activity? activity,
        string operation,
        long startedAt,
        double requestCharge)
    {
        activity.RecordSuccess(requestCharge);
        Meter.RecordOperation(
            operation,
            "success",
            Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds,
            requestCharge);
        _logger.OperationCompleted(
            operation,
            SupportedContainers.CourseAudits,
            requestCharge);
    }

    private void RecordNotFound(
        Activity? activity,
        string operation,
        long startedAt,
        double requestCharge,
        string id)
    {
        activity.RecordNotFound();
        Meter.RecordOperation(
            operation,
            "not_found",
            Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds,
            requestCharge);
        _logger.DocumentNotFound(operation, SupportedContainers.CourseAudits, id);
    }

    private void RecordFailure(
        Activity? activity,
        string operation,
        long startedAt,
        Exception exception)
    {
        activity.RecordFailure(exception);
        var failureType = exception is OperationCanceledException
            ? "cancelled"
            : exception.GetType().Name;

        Meter.RecordOperation(
            operation,
            failureType == "cancelled" ? "cancelled" : "failure",
            Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds,
            exception is CosmosException cosmosException
                ? cosmosException.RequestCharge
                : null);
        Meter.RecordFailure(operation, failureType);

        if (exception is OperationCanceledException)
        {
            _logger.OperationCancelled(
                operation,
                SupportedContainers.CourseAudits,
                exception);
            return;
        }

        if (exception is CosmosException cosmosError)
        {
            var statusCode = (int)cosmosError.StatusCode;
            if (statusCode >= 500)
            {
                _logger.OperationError(
                    operation,
                    SupportedContainers.CourseAudits,
                    statusCode,
                    cosmosError.ActivityId,
                    cosmosError.RequestCharge,
                    cosmosError);
            }
            else
            {
                _logger.OperationWarning(
                    operation,
                    SupportedContainers.CourseAudits,
                    statusCode,
                    cosmosError.ActivityId,
                    cosmosError.RequestCharge,
                    cosmosError);
            }

            return;
        }

        _logger.UnexpectedOperationFailure(
            operation,
            SupportedContainers.CourseAudits,
            exception);
    }

}
