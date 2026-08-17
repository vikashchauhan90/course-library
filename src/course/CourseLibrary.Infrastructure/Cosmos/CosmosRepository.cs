using CourseLibrary.Domain.Abstractions;
using CourseLibrary.Domain.Entities;
using CourseLibrary.Domain.Models;
using CourseLibrary.Infrastructure.Configuration.Cosmos;
using CourseLibrary.Infrastructure.Cosmos.Extensions;
using CourseLibrary.Infrastructure.Observability.Traces;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;
using System.Reflection;

namespace CourseLibrary.Infrastructure.Cosmos;

public class CosmosRepository<TDocument>
    : ICosmosRepository<TDocument>
    where TDocument : ICosmosPartitioned
{
    private readonly Lazy<Container> _container;
    private readonly ILogger<CosmosRepository<TDocument>> _logger;
    private readonly string ContainerName;
    public CosmosRepository(
        CosmosClient client,
        IOptions<CosmosOptions> options,
        ILogger<CosmosRepository<TDocument>> logger)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options.Value);

        var containerName =
        typeof(TDocument)
            .GetCustomAttribute<CosmosContainerAttribute>()
            ?.ContainerName
        ?? throw new InvalidOperationException(
            $"Cosmos container attribute is missing for document type '{typeof(TDocument).Name}'.");

        ContainerName = containerName;
        _logger = logger;
        _container = new Lazy<Container>(
            () => client.GetContainer(
                options.Value.DatabaseName,
                containerName),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    private Container Container =>
        _container.Value;


    public async Task<TDocument?> GetByIdAsync(
        string id,
        string partitionKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);

        using var activity =
         ActivitySources.Infrastructure.StartActivity(
             "Cosmos.GetItem",
             ActivityKind.Client);

        activity.SetCosmosOperation(
            "ReadItem",
            ContainerName);

        try
        {
            var response = await Container.ReadItemAsync<TDocument>(
                id,
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
                 ContainerName,
                 id);


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
            ContainerName);

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
            ContainerName);

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
        ArgumentException.ThrowIfNullOrWhiteSpace(
            item.PartitionKeyValue);


        using var activity =
        ActivitySources.Infrastructure.StartActivity(
            "Cosmos.UpsertItem",
            ActivityKind.Client);

        activity.SetCosmosOperation(
            "UpsertItem",
            ContainerName);

        try
        {
            var response =
            await Container.UpsertItemAsync(
                item,
                new PartitionKey(
                    item.PartitionKeyValue),
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
        string id,
        string partitionKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);

        using var activity =
       ActivitySources.Infrastructure.StartActivity(
           "Cosmos.DeleteItem",
           ActivityKind.Client);

        activity.SetCosmosOperation(
            "DeleteItem",
            ContainerName);

        try
        {
            var response =
           await Container.DeleteItemAsync<TDocument>(
               id,
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
                 ContainerName,
                 id);

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
                ContainerName,
                statusCode,
                exception.ActivityId,
                exception.RequestCharge,
                exception);

            return;
        }

        _logger.OperationWarning(
            operation,
            ContainerName,
            statusCode,
            exception.ActivityId,
            exception.RequestCharge,
            exception);
    }
}