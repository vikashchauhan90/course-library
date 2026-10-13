using CourseLibrary.Core.Results;
using CourseLibrary.Platform.Persistence.CosmosDB.Configurations;
using CourseLibrary.Platform.Persistence.CosmosDB.Observability;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;

namespace CourseLibrary.Platform.Persistence.CosmosDB.Repositories;

public class CosmosRepository<TDocument, TKey>
    : ICosmosRepository<TDocument, TKey>
    where TDocument : class
    where TKey : notnull
{
    private readonly Lazy<Container> _container;
    private readonly ILogger<CosmosRepository<TDocument, TKey>> _logger;
    private readonly ICosmosDocumentConfiguration<TDocument> _configuration;
    private readonly CosmosOptions _options;
    private readonly CosmosClient _client;
    private readonly CosmosActivitySource _activitySource;
    private static readonly CosmosMeter Metrics = CosmosMeter.Shared;

    public CosmosRepository(
        CosmosClient client,
        IOptions<CosmosOptions> options,
        ICosmosDocumentConfiguration<TDocument> configuration,
        CosmosActivitySource activitySource,
        ILogger<CosmosRepository<TDocument, TKey>> logger)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(activitySource);
        ArgumentNullException.ThrowIfNull(logger);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            options.Value.DatabaseName);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            configuration.ContainerName);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            configuration.PartitionKeyPath);

        _configuration = configuration;
        _options = options.Value;
        _client = client;
        _activitySource = activitySource;
        _logger = logger;

        _container = new Lazy<Container>(
            () => _client.GetContainer(
                _options.DatabaseName,
                _configuration.ContainerName),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    private Container Container =>
        _container.Value;


    public async Task<TDocument?> GetByIdAsync(
        TKey id,
        string partitionKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);

        using var activity =
         _activitySource.StartActivity(
             "Cosmos.GetItem",
             ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();

        activity.SetCosmosOperation(
            "ReadItem",
            _configuration.ContainerName,
            Container.Database.Id);

        try
        {
            var response = await Container.ReadItemAsync<TDocument>(
                id.ToString(),
                new PartitionKey(partitionKey),
                cancellationToken: cancellationToken);

            RecordSuccess(activity, "ReadItem", startedAt, response.RequestCharge);

            return response.Resource;
        }
        catch (CosmosException ex) when (
            ex.StatusCode == HttpStatusCode.NotFound)
        {
            RecordNotFound(
                activity,
                "ReadItem",
                startedAt,
                ex.RequestCharge,
                id.ToString() ?? string.Empty);

            return default;
        }
        catch (CosmosException ex)
        {
            RecordFailure(activity, "ReadItem", startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            RecordFailure(activity, "ReadItem", startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            RecordFailure(activity, "ReadItem", startedAt, ex);
            throw;
        }
    }

    public async Task<IReadOnlyList<TDocument>> QueryAsync(
        QueryDefinition query,
        string? partitionKey = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        using var activity =
        _activitySource.StartActivity(
            "Cosmos.Query",
            ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();

        activity.SetCosmosOperation(
            "Query",
            _configuration.ContainerName,
            Container.Database.Id);

        try
        {
            var requestOptions = new QueryRequestOptions();

            if (!string.IsNullOrWhiteSpace(partitionKey))
            {
                requestOptions.PartitionKey = new PartitionKey(partitionKey);
            }

            using var iterator =
                Container.GetItemQueryIterator<TDocument>(
                    queryDefinition: query,
                    requestOptions: requestOptions);

            var results = new List<TDocument>();
            double totalRequestCharge = 0;
            while (iterator.HasMoreResults)
            {
                var response =
                    await iterator.ReadNextAsync(
                        cancellationToken);

                results.AddRange(response.Resource);
                totalRequestCharge += response.RequestCharge;
            }

            RecordSuccess(activity, "Query", startedAt, totalRequestCharge);

            activity?.SetTag(
                CosmosActivityTagConstants.ResultCount,
                results.Count);

            return results;
        }
        catch (CosmosException ex)
        {
            RecordFailure(activity, "Query", startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            RecordFailure(activity, "Query", startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            RecordFailure(activity, "Query", startedAt, ex);
            throw;
        }
    }

    public async Task<PageResult<TDocument>> QueryPageAsync(
        QueryDefinition query,
        string? partitionKey = null,
        string? continuationToken = null,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (pageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                pageSize,
                "Page size must be between 1 and 100.");
        }

        using var activity =
       _activitySource.StartActivity(
           "Cosmos.QueryPage",
           ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();

        activity.SetCosmosOperation(
            "QueryPage",
            _configuration.ContainerName,
            Container.Database.Id);

        activity?.SetTag(
        CosmosActivityTagConstants.PageSize,
        pageSize);

        activity?.SetTag(
            CosmosActivityTagConstants.HasContinuationToken,
            !string.IsNullOrWhiteSpace(continuationToken));

        try
        {
            var requestOptions = new QueryRequestOptions
            {
                MaxItemCount = pageSize
            };

            if (!string.IsNullOrWhiteSpace(partitionKey))
            {
                requestOptions.PartitionKey = new PartitionKey(partitionKey);
            }

            using var iterator =
                Container.GetItemQueryIterator<TDocument>(
                    queryDefinition: query,
                    continuationToken: continuationToken,
                    requestOptions: requestOptions);

            if (!iterator.HasMoreResults)
            {
                activity?.SetTag(CosmosActivityTagConstants.ResultCount, 0);
                RecordSuccess(activity, "QueryPage", startedAt, 0);

                return new PageResult<TDocument>(
                    [],
                    null,
                    false);
            }

            var response =
                await iterator.ReadNextAsync(
                    cancellationToken);

            var items = response.Resource.ToList();

            var nextToken = response.ContinuationToken;

            activity?.SetTag(CosmosActivityTagConstants.ResultCount, items.Count);
            activity?.SetTag(
                CosmosActivityTagConstants.HasNextPage,
                !string.IsNullOrWhiteSpace(nextToken));
            RecordSuccess(activity, "QueryPage", startedAt, response.RequestCharge);

            return new PageResult<TDocument>(
                items,
                nextToken,
                !string.IsNullOrWhiteSpace(nextToken));
        }
        catch (CosmosException ex)
        {
            RecordFailure(activity, "QueryPage", startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            RecordFailure(activity, "QueryPage", startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            RecordFailure(activity, "QueryPage", startedAt, ex);
            throw;
        }
    }

    public async Task UpsertAsync(
        TDocument item,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        var partitionKey = _configuration.GetPartitionKey(item);
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);



        using var activity =
        _activitySource.StartActivity(
            "Cosmos.UpsertItem",
            ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();

        activity.SetCosmosOperation(
            "UpsertItem",
            _configuration.ContainerName,
            Container.Database.Id);

        try
        {
            var response =
            await Container.UpsertItemAsync(
                item,
                new PartitionKey(
                    partitionKey),
                cancellationToken: cancellationToken);

            RecordSuccess(activity, "UpsertItem", startedAt, response.RequestCharge);
        }
        catch (CosmosException ex)
        {
            RecordFailure(activity, "UpsertItem", startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            RecordFailure(activity, "UpsertItem", startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            RecordFailure(activity, "UpsertItem", startedAt, ex);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(
        TKey id,
        string partitionKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);

        using var activity =
       _activitySource.StartActivity(
           "Cosmos.DeleteItem",
           ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();

        activity.SetCosmosOperation(
            "DeleteItem",
            _configuration.ContainerName,
            Container.Database.Id);

        try
        {
            var response =
           await Container.DeleteItemAsync<TDocument>(
               id.ToString(),
               new PartitionKey(partitionKey),
               cancellationToken: cancellationToken);

            RecordSuccess(activity, "DeleteItem", startedAt, response.RequestCharge);

            return true;
        }
        catch (CosmosException ex) when (
            ex.StatusCode == HttpStatusCode.NotFound)
        {
            RecordNotFound(
                activity,
                "DeleteItem",
                startedAt,
                ex.RequestCharge,
                id.ToString() ?? string.Empty);

            return false;
        }
        catch (CosmosException ex)
        {
            RecordFailure(activity, "DeleteItem", startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            RecordFailure(activity, "DeleteItem", startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            RecordFailure(activity, "DeleteItem", startedAt, ex);
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
        Metrics.RecordOperation(
            operation,
            "success",
            Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds,
            requestCharge);
        _logger.OperationCompleted(
            operation,
            _configuration.ContainerName,
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
        Metrics.RecordOperation(
            operation,
            "not_found",
            Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds,
            requestCharge);
        _logger.DocumentNotFound(operation, _configuration.ContainerName, id);
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

        Metrics.RecordOperation(
            operation,
            failureType == "cancelled" ? "cancelled" : "failure",
            Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds,
            exception is CosmosException cosmosException
                ? cosmosException.RequestCharge
                : null);
        Metrics.RecordFailure(operation, failureType);

        if (exception is OperationCanceledException)
        {
            _logger.OperationCancelled(
                operation,
                _configuration.ContainerName,
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
                    _configuration.ContainerName,
                    statusCode,
                    cosmosError.ActivityId,
                    cosmosError.RequestCharge,
                    cosmosError);
            }
            else
            {
                _logger.OperationWarning(
                    operation,
                    _configuration.ContainerName,
                    statusCode,
                    cosmosError.ActivityId,
                    cosmosError.RequestCharge,
                    cosmosError);
            }

            return;
        }

        _logger.UnexpectedOperationFailure(
            operation,
            _configuration.ContainerName,
            exception);
    }
}