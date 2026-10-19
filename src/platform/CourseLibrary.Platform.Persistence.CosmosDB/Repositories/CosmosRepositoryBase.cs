using CourseLibrary.Core.Results;
using CourseLibrary.Platform.Persistence.CosmosDB.Configurations;
using CourseLibrary.Platform.Persistence.CosmosDB.Observability;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;

namespace CourseLibrary.Platform.Persistence.CosmosDB.Repositories;

public abstract class CosmosRepositoryBase<TDocument, TKey>
    : ICosmosRepository<TDocument, TKey>
    where TDocument : class
    where TKey : notnull
{
    private readonly Lazy<Container> _container;
    private readonly ILogger _logger;
    private readonly ICosmosDocumentConfiguration<TDocument> _configuration;
    private readonly CosmosOptions _options;
    private readonly CosmosClient _client;
    private static readonly CosmosActivitySource ActivitySource = CosmosActivitySource.Shared;
    private static readonly CosmosMeter Meter = CosmosMeter.Shared;

    public CosmosRepositoryBase(
        CosmosClient client,
        IOptions<CosmosOptions> options,
        ICosmosDocumentConfiguration<TDocument> configuration,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(configuration);
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
        _logger = logger;

        _container = new Lazy<Container>(
            () => _client.GetContainer(
                _options.DatabaseName,
                _configuration.ContainerName),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    protected Container Container =>
        _container.Value;
    public async Task<TDocument?> GetByIdAsync(
        TKey id,
        string partitionKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);

        using var activity =
         ActivitySource.StartActivity(
            CosmosActivityNames.GetItem,
             ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();

        activity.SetCosmosOperation(
            OperationNames.GetItem,
            _configuration.ContainerName,
            Container.Database.Id);

        try
        {
            var response = await Container.ReadItemAsync<TDocument>(
                id.ToString(),
                new PartitionKey(partitionKey),
                cancellationToken: cancellationToken);

            RecordSuccess(activity, OperationNames.GetItem, startedAt, response.RequestCharge);

            return response.Resource;
        }
        catch (CosmosException ex) when (
            ex.StatusCode == HttpStatusCode.NotFound)
        {
            RecordNotFound(
                activity,
                OperationNames.GetItem,
                startedAt,
                ex.RequestCharge,
                id.ToString() ?? string.Empty);

            return default;
        }
        catch (CosmosException ex)
        {
            RecordFailure(activity, OperationNames.GetItem, startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            RecordFailure(activity, OperationNames.GetItem, startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            RecordFailure(activity, OperationNames.GetItem, startedAt, ex);
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
        ActivitySource.StartActivity(
            CosmosActivityNames.QueryItems,
            ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();

        activity.SetCosmosOperation(
            OperationNames.QueryItems,
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

            RecordSuccess(activity, OperationNames.QueryItems, startedAt, totalRequestCharge);

            activity?.SetTag(
                CosmosActivityTagConstants.ResultCount,
                results.Count);

            return results;
        }
        catch (CosmosException ex)
        {
            RecordFailure(activity, OperationNames.QueryItems, startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            RecordFailure(activity, OperationNames.QueryItems, startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            RecordFailure(activity, OperationNames.QueryItems, startedAt, ex);
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
       ActivitySource.StartActivity(
           CosmosActivityNames.QueryPage,
           ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();

        activity.SetCosmosOperation(
            OperationNames.QueryPage,
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
                RecordSuccess(activity, OperationNames.QueryPage, startedAt, 0);

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
            RecordSuccess(activity, OperationNames.QueryPage, startedAt, response.RequestCharge);

            return new PageResult<TDocument>(
                items,
                nextToken,
                !string.IsNullOrWhiteSpace(nextToken));
        }
        catch (CosmosException ex)
        {
            RecordFailure(activity, OperationNames.QueryPage, startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            RecordFailure(activity, OperationNames.QueryPage, startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            RecordFailure(activity, OperationNames.QueryPage, startedAt, ex);
            throw;
        }
    }

    public async Task<IReadOnlyList<TDocument>> GetManyAsync(
        IReadOnlyList<(TKey Id, string PartitionKey)> items,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (items.Any(static item => string.IsNullOrWhiteSpace(item.PartitionKey)))
        {
            throw new ArgumentException(
                "Every item must include a partition key.",
                nameof(items));
        }

        using var activity = ActivitySource.StartActivity(
            CosmosActivityNames.ReadManyItems,
            ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();
        activity.SetCosmosOperation(
            OperationNames.ReadManyItems,
            _configuration.ContainerName,
            Container.Database.Id);

        try
        {
            if (items.Count == 0)
            {
                activity?.SetTag(CosmosActivityTagConstants.ResultCount, 0);
                RecordSuccess(activity, OperationNames.ReadManyItems, startedAt, 0);
                return [];
            }

            var identifiers = items
                .Select(static item => (
                    item.Id.ToString() ?? string.Empty,
                    new PartitionKey(item.PartitionKey)))
                .ToList();
            var response = await Container.ReadManyItemsAsync<TDocument>(
                identifiers,
                cancellationToken: cancellationToken);
            var results = response.Resource.ToList();

            activity?.SetTag(CosmosActivityTagConstants.ResultCount, results.Count);
            RecordSuccess(activity, OperationNames.ReadManyItems, startedAt, response.RequestCharge);
            return results;
        }
        catch (CosmosException ex)
        {
            RecordFailure(activity, OperationNames.ReadManyItems, startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            RecordFailure(activity, OperationNames.ReadManyItems, startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            RecordFailure(activity, OperationNames.ReadManyItems, startedAt, ex);
            throw;
        }
    }

    public async Task CreateAsync(
        TDocument item,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        var partitionKey = _configuration.GetPartitionKey(item);
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);

        using var activity = ActivitySource.StartActivity(
            CosmosActivityNames.CreateItem,
            ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();
        activity.SetCosmosOperation(
            OperationNames.CreateItem,
            _configuration.ContainerName,
            Container.Database.Id);

        try
        {
            var response = await Container.CreateItemAsync(
                item,
                new PartitionKey(partitionKey),
                cancellationToken: cancellationToken);
            RecordSuccess(activity, OperationNames.CreateItem, startedAt, response.RequestCharge);
        }
        catch (CosmosException ex)
        {
            RecordFailure(activity, OperationNames.CreateItem, startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            RecordFailure(activity, OperationNames.CreateItem, startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            RecordFailure(activity, OperationNames.CreateItem, startedAt, ex);
            throw;
        }
    }

    public Task ReplaceAsync(
        TKey id,
        TDocument item,
        string partitionKey,
        CancellationToken cancellationToken = default)
        => ReplaceCoreAsync(id, item, partitionKey, etag: null, cancellationToken);

    public Task ReplaceAsync(
        TKey id,
        TDocument item,
        string partitionKey,
        string etag,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(etag);
        return ReplaceCoreAsync(id, item, partitionKey, etag, cancellationToken);
    }

    private async Task ReplaceCoreAsync(
        TKey id,
        TDocument item,
        string partitionKey,
        string? etag,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);
        ArgumentNullException.ThrowIfNull(id);

        using var activity = ActivitySource.StartActivity(
            CosmosActivityNames.ReplaceItem,
            ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();
        activity.SetCosmosOperation(
            OperationNames.ReplaceItem,
            _configuration.ContainerName,
            Container.Database.Id);

        try
        {
            var requestOptions = etag is null
                ? null
                : new ItemRequestOptions { IfMatchEtag = etag };
            var response = await Container.ReplaceItemAsync(
                item,
                id.ToString(),
                new PartitionKey(partitionKey),
                requestOptions,
                cancellationToken);
            RecordSuccess(activity, OperationNames.ReplaceItem, startedAt, response.RequestCharge);
        }
        catch (CosmosException ex)
        {
            RecordFailure(activity, OperationNames.ReplaceItem, startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            RecordFailure(activity, OperationNames.ReplaceItem, startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            RecordFailure(activity, OperationNames.ReplaceItem, startedAt, ex);
            throw;
        }
    }

    public async Task PatchAsync(
        TKey id,
        string partitionKey,
        IReadOnlyList<PatchOperation> operations,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);
        ArgumentNullException.ThrowIfNull(operations);
        if (operations.Count == 0)
        {
            throw new ArgumentException(
                "At least one patch operation is required.",
                nameof(operations));
        }

        using var activity = ActivitySource.StartActivity(
            CosmosActivityNames.PatchItem,
            ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();
        activity.SetCosmosOperation(
            OperationNames.PatchItem,
            _configuration.ContainerName,
            Container.Database.Id);
        activity?.SetTag(CosmosActivityTagConstants.PatchOperationCount, operations.Count);

        try
        {
            var response = await Container.PatchItemAsync<TDocument>(
                id.ToString(),
                new PartitionKey(partitionKey),
                operations,
                cancellationToken: cancellationToken);
            RecordSuccess(activity, OperationNames.PatchItem, startedAt, response.RequestCharge);
        }
        catch (CosmosException ex)
        {
            RecordFailure(activity, OperationNames.PatchItem, startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            RecordFailure(activity, OperationNames.PatchItem, startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            RecordFailure(activity, OperationNames.PatchItem, startedAt, ex);
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
        ActivitySource.StartActivity(
            CosmosActivityNames.UpsertItem,
            ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();

        activity.SetCosmosOperation(
            OperationNames.UpsertItem,
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

            RecordSuccess(activity, OperationNames.UpsertItem, startedAt, response.RequestCharge);
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

    public async Task<bool> DeleteAsync(
        TKey id,
        string partitionKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);

        using var activity =
       ActivitySource.StartActivity(
           CosmosActivityNames.DeleteItem,
           ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();

        activity.SetCosmosOperation(
            OperationNames.DeleteItem,
            _configuration.ContainerName,
            Container.Database.Id);

        try
        {
            var response =
           await Container.DeleteItemAsync<TDocument>(
               id.ToString(),
               new PartitionKey(partitionKey),
               cancellationToken: cancellationToken);

            RecordSuccess(activity, OperationNames.DeleteItem, startedAt, response.RequestCharge);

            return true;
        }
        catch (CosmosException ex) when (
            ex.StatusCode == HttpStatusCode.NotFound)
        {
            RecordNotFound(
                activity,
                OperationNames.DeleteItem,
                startedAt,
                ex.RequestCharge,
                id.ToString() ?? string.Empty);

            return false;
        }
        catch (CosmosException ex)
        {
            RecordFailure(activity, OperationNames.DeleteItem, startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            RecordFailure(activity, OperationNames.DeleteItem, startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            RecordFailure(activity, OperationNames.DeleteItem, startedAt, ex);
            throw;
        }
    }

    public Task<TDocument?> FirstOrDefaultAsync(
        QueryDefinition query,
        string? partitionKey = null,
        CancellationToken cancellationToken = default)
        => FindOneAsync(
            query,
            partitionKey,
            OperationNames.FirstItem,
            CosmosActivityNames.FirstItem,
            single: false,
            cancellationToken);

    public Task<TDocument?> SingleOrDefaultAsync(
        QueryDefinition query,
        string? partitionKey = null,
        CancellationToken cancellationToken = default)
        => FindOneAsync(
            query,
            partitionKey,
            OperationNames.SingleItem,
            CosmosActivityNames.SingleItem,
            single: true,
            cancellationToken);

    private async Task<TDocument?> FindOneAsync(
        QueryDefinition query,
        string? partitionKey,
        string operation,
        string activityName,
        bool single,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        using var activity = ActivitySource.StartActivity(activityName, ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();
        activity.SetCosmosOperation(
            operation,
            _configuration.ContainerName,
            Container.Database.Id);

        try
        {
            var limit = single ? 2 : 1;
            var requestOptions = new QueryRequestOptions { MaxItemCount = limit };
            if (!string.IsNullOrWhiteSpace(partitionKey))
            {
                requestOptions.PartitionKey = new PartitionKey(partitionKey);
            }

            using var iterator = Container.GetItemQueryIterator<TDocument>(
                queryDefinition: query,
                requestOptions: requestOptions);
            var results = new List<TDocument>(limit);
            double totalRequestCharge = 0;

            while (iterator.HasMoreResults && results.Count < limit)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                results.AddRange(response.Resource.Take(limit - results.Count));
                totalRequestCharge += response.RequestCharge;
            }

            activity?.SetTag(CosmosActivityTagConstants.ResultCount, results.Count);
            if (single && results.Count > 1)
            {
                throw new InvalidOperationException("The query returned more than one document.");
            }

            RecordSuccess(activity, operation, startedAt, totalRequestCharge);
            return results.Count == 0 ? default : results[0];
        }
        catch (CosmosException ex)
        {
            RecordFailure(activity, operation, startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            RecordFailure(activity, operation, startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            RecordFailure(activity, operation, startedAt, ex);
            throw;
        }
    }

    public async Task ExecuteBatchAsync(
        string partitionKey,
        Action<TransactionalBatch> configure,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);
        ArgumentNullException.ThrowIfNull(configure);

        using var activity = ActivitySource.StartActivity(
            CosmosActivityNames.ExecuteBatch,
            ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();
        activity.SetCosmosOperation(
            OperationNames.ExecuteBatch,
            _configuration.ContainerName,
            Container.Database.Id);

        try
        {
            var batch = Container.CreateTransactionalBatch(new PartitionKey(partitionKey));
            configure(batch);
            using var response = await batch.ExecuteAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new CosmosException(
                    response.ErrorMessage ?? "The Cosmos DB transactional batch failed.",
                    response.StatusCode,
                    0,
                    response.ActivityId,
                    response.RequestCharge);
            }

            RecordSuccess(activity, OperationNames.ExecuteBatch, startedAt, response.RequestCharge);
        }
        catch (CosmosException ex)
        {
            RecordFailure(activity, OperationNames.ExecuteBatch, startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            RecordFailure(activity, OperationNames.ExecuteBatch, startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            RecordFailure(activity, OperationNames.ExecuteBatch, startedAt, ex);
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
        Meter.RecordOperation(
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