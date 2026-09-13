using CourseLibrary.Infrastructure.Configuration.Cosmos;
using CourseLibrary.Infrastructure.Cosmos.Configurations;
using CourseLibrary.Infrastructure.Cosmos.Extensions;
using CourseLibrary.Infrastructure.Observability.Traces;
using CourseLibrary.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;

namespace CourseLibrary.Infrastructure.Cosmos;

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
    public CosmosRepository(
        CosmosClient client,
        IOptions<CosmosOptions> options,
        ICosmosDocumentConfiguration<TDocument> configuration,
        ILogger<CosmosRepository<TDocument, TKey>> logger)
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

    private Container Container =>
        _container.Value;


    public async Task<TDocument?> GetByIdAsync(
        TKey id,
        string partitionKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);

        using var activity =
         ActivitySources.Infrastructure.StartActivity(
             "Cosmos.GetItem",
             ActivityKind.Client);

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

            activity.RecordSuccess(
            response.RequestCharge);

            return response.Resource;
        }
        catch (CosmosException ex) when (
            ex.StatusCode == HttpStatusCode.NotFound)
        {
            activity?.SetTag(
             "http.response.status_code",
             StatusCodes.Status404NotFound);

            _logger.DocumentNotFound(
                 "ReadItem",
                 _configuration.ContainerName,
                 id.ToString() ?? string.Empty);


            return default;
        }
        catch (CosmosException ex)
        {
            activity.RecordFailure(ex);

            LogCosmosFailure(
                 "ReadItem",
                 ex);

            throw ex.ToApplicationException();
        }
    }

    public async Task<IReadOnlyList<TDocument>> QueryAsync(
        QueryDefinition query,
        string? partitionKey = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        using var activity =
        ActivitySources.Infrastructure.StartActivity(
            "Cosmos.Query",
            ActivityKind.Client);

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

            activity.RecordSuccess(
          totalRequestCharge);

            activity?.SetTag(
                "cosmos.result_count",
                results.Count);

            return results;
        }
        catch (CosmosException ex)
        {
            activity.RecordFailure(ex);

            LogCosmosFailure(
                "Query",
                ex);
            throw ex.ToApplicationException();
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
       ActivitySources.Infrastructure.StartActivity(
           "Cosmos.QueryPage",
           ActivityKind.Client);

        activity.SetCosmosOperation(
            "Cosmos.QueryPage",
            _configuration.ContainerName,
            Container.Database.Id);

        activity?.SetTag(
        "cosmos.page_size",
        pageSize);

        activity?.SetTag(
            "cosmos.has_continuation_token",
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

            activity?.SetTag(
            "cosmos.result_count",
            items.Count);

            activity?.SetTag(
                "cosmos.has_next_page",
                !string.IsNullOrWhiteSpace(nextToken));

            activity?.RecordSuccess(response.RequestCharge);

            return new PageResult<TDocument>(
                items,
                nextToken,
                !string.IsNullOrWhiteSpace(nextToken));
        }
        catch (CosmosException ex)
        {
            activity.RecordFailure(ex);

            LogCosmosFailure(
                "QueryPage",
                ex);
            throw ex.ToApplicationException();
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
        ActivitySources.Infrastructure.StartActivity(
            "Cosmos.UpsertItem",
            ActivityKind.Client);

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

            activity.RecordSuccess(
                response.RequestCharge);
        }
        catch (CosmosException ex)
        {
            activity.RecordFailure(ex);

            LogCosmosFailure(
               "UpsertItem",
               ex);

            throw ex.ToApplicationException();
        }
    }

    public async Task<bool> DeleteAsync(
        TKey id,
        string partitionKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);

        using var activity =
       ActivitySources.Infrastructure.StartActivity(
           "Cosmos.DeleteItem",
           ActivityKind.Client);

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

            activity.RecordSuccess(
                response.RequestCharge);

            return true;
        }
        catch (CosmosException ex) when (
            ex.StatusCode == HttpStatusCode.NotFound)
        {
            activity?.SetTag(
            "http.response.status_code",
            StatusCodes.Status404NotFound);

            _logger.DocumentNotFound(
                 "DeleteItem",
                 _configuration.ContainerName,
                 id.ToString() ?? string.Empty);

            return false;
        }
        catch (CosmosException ex)
        {
            activity.RecordFailure(ex);

            LogCosmosFailure(
               "DeleteItem",
               ex);
            throw ex.ToApplicationException();
        }
    }

    protected void LogCosmosFailure(
        string operation,
        CosmosException exception)
    {
        var statusCode = (int)exception.StatusCode;

        if (statusCode >= 500)
        {
            _logger.OperationError(
                operation,
                _configuration.ContainerName,
                statusCode,
                exception.ActivityId,
                exception.RequestCharge,
                exception);

            return;
        }

        _logger.OperationWarning(
            operation,
            _configuration.ContainerName,
            statusCode,
            exception.ActivityId,
            exception.RequestCharge,
            exception);
    }
}