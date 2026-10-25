using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Course.Contracts.DTOs.Courses.Audits;
using CourseLibrary.Course.Persistence.Core;
using CourseLibrary.Course.Persistence.Cosmos;
using CourseLibrary.Course.Persistence.Entities;
using CourseLibrary.Course.Persistence.Observability;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;

namespace CourseLibrary.Course.Persistence.Repositories.Courses.Audits;

internal sealed class CourseAuditRepository : ICourseAuditRepository
{
    private readonly ICosmosContainerFactory _containerFactory;
    private readonly ILogger<CourseAuditRepository> _logger;
    private static readonly CosmosActivitySource ActivitySource = CosmosActivitySource.Shared;
    public CourseAuditRepository(
    ICosmosContainerFactory containerFactory,
    ILogger<CourseAuditRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(containerFactory);
        ArgumentNullException.ThrowIfNull(logger);
        _containerFactory = containerFactory;
        _logger = logger;
    }


    public async Task<CourseAuditResponse?> GetByIdAsync(
    string courseId,
    string id,
    CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(courseId);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        using var activity = ActivitySource.StartActivity(
            CosmosActivityNames.GetItem,
            ActivityKind.Client);

        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var container = _containerFactory.GetContainer(
                SupportedContainers.CourseAudits);

            activity.SetCosmosOperation(
                OperationNames.GetItem,
                SupportedContainers.CourseAudits,
                container.Database.Id);

            var partitionKey = CoursePartitionKeyFormats.ForCourse(courseId);

            var response = await container.ReadItemAsync<CourseAuditEntry>(
                id,
                new PartitionKey(partitionKey),
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            CosmosOperationObserver.RecordSuccess(
                activity,
                _logger,
                OperationNames.GetItem,
                SupportedContainers.CourseAudits,
                startedAt,
                response.RequestCharge);

            return CourseAuditMapper.ToResponse(response.Resource);
        }
        catch (CosmosException ex) when (
            ex.StatusCode == HttpStatusCode.NotFound)
        {
            CosmosOperationObserver.RecordNotFound(
                activity,
                _logger,
                OperationNames.GetItem,
                SupportedContainers.CourseAudits,
                startedAt,
                ex.RequestCharge,
                id);

            return null;
        }
        catch (CosmosException ex)
        {
            CosmosOperationObserver.RecordFailure(
                activity,
                _logger,
                OperationNames.GetItem,
                SupportedContainers.CourseAudits,
                startedAt,
                ex);

            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            CosmosOperationObserver.RecordFailure(
                activity,
                _logger,
                OperationNames.GetItem,
                SupportedContainers.CourseAudits,
                startedAt,
                ex);

            throw;
        }
        catch (Exception ex)
        {
            CosmosOperationObserver.RecordFailure(
                activity,
                _logger,
                OperationNames.GetItem,
                SupportedContainers.CourseAudits,
                startedAt,
                ex);

            throw;
        }
    }
    public async Task<CourseAuditResponse> AddAsync(
        CourseAuditRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var activity =
        ActivitySource.StartActivity(
            CosmosActivityNames.CreateItem,
            ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var container = _containerFactory
            .GetContainer(SupportedContainers.CourseAudits);

            activity.SetCosmosOperation(
            OperationNames.CreateItem,
            SupportedContainers.CourseAudits,
            container.Database.Id);

            var entity = CourseAuditMapper.ToEntity(request);

            ItemRequestOptions requestOptions = new ItemRequestOptions
            {
                ConsistencyLevel = ConsistencyLevel.Eventual,
            };

            var response = await container
            .CreateItemAsync(
                entity,
                partitionKey: new PartitionKey(entity.PartitionKey),
                requestOptions: requestOptions,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

            CosmosOperationObserver
                .RecordSuccess(
                activity,
                _logger,
                OperationNames.CreateItem,
                SupportedContainers.CourseAudits,
                startedAt,
                response.RequestCharge);

            return CourseAuditMapper.ToResponse(entity);

        }
        catch (CosmosException ex)
        {
            CosmosOperationObserver
                .RecordFailure(
                activity,
                _logger,
                OperationNames.CreateItem,
                SupportedContainers.CourseAudits,
                startedAt,
                ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            CosmosOperationObserver
                .RecordFailure(
                activity,
                _logger,
                OperationNames.CreateItem,
                SupportedContainers.CourseAudits,
                startedAt,
                ex);
            throw;
        }
        catch (Exception ex)
        {
            CosmosOperationObserver
                .RecordFailure(
                activity,
                _logger,
                OperationNames.CreateItem,
                SupportedContainers.CourseAudits,
                startedAt,
                ex);
            throw;
        }

    }

    public async Task<PageResult<CourseAuditResponse>> GetPageAsync(
        string courseId,
        string? continuationToken = null,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(courseId);

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

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var container = _containerFactory
            .GetContainer(SupportedContainers.CourseAudits);

            activity.SetCosmosOperation(
         OperationNames.QueryPage,
         SupportedContainers.CourseAudits,
         container.Database.Id);

            activity?.SetTag(
            CosmosActivityTagConstants.PageSize,
            pageSize);

            activity?.SetTag(
                CosmosActivityTagConstants.HasContinuationToken,
                !string.IsNullOrWhiteSpace(continuationToken));


            var partitionKeyValue =
            CoursePartitionKeyFormats
            .ForCourse(courseId);

            var requestOptions = new QueryRequestOptions
            {
                MaxItemCount = pageSize,
                PartitionKey = new PartitionKey(partitionKeyValue)
            };

            var query = new QueryDefinition(
                """
                SELECT * FROM c
                WHERE c.courseId = @courseId
                ORDER BY c.createdAt DESC
                """)
                .WithParameter("@courseId", courseId);

            using var iterator =
                container.GetItemQueryIterator<CourseAuditEntry>(
                    queryDefinition: query,
                    continuationToken: continuationToken,
                    requestOptions: requestOptions);

            var response = await iterator.ReadNextAsync(cancellationToken)
           .ConfigureAwait(false);


            var items = response.Resource
          .Select(CourseAuditMapper.ToResponse)
          .ToList();

            var nextToken = response.ContinuationToken;
            var hasNextPage = !string.IsNullOrWhiteSpace(nextToken);

            activity?.SetTag(
                CosmosActivityTagConstants.ResultCount,
                items.Count);

            activity?.SetTag(
                CosmosActivityTagConstants.HasNextPage,
                hasNextPage);

            CosmosOperationObserver
                  .RecordSuccess(
                  activity,
                  _logger,
                  OperationNames.QueryPage,
                  SupportedContainers.CourseAudits,
                  startedAt,
                  response.RequestCharge);

            return new PageResult<CourseAuditResponse>(
                items,
                nextToken,
                hasNextPage);
        }
        catch (CosmosException ex)
        {
            CosmosOperationObserver
                .RecordFailure(
                activity,
                _logger,
                OperationNames.QueryPage,
                SupportedContainers.CourseAudits,
                startedAt,
                ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            CosmosOperationObserver
                .RecordFailure(
                activity,
                _logger,
                OperationNames.QueryPage,
                SupportedContainers.CourseAudits,
                startedAt,
                ex);
            throw;
        }
        catch (Exception ex)
        {
            CosmosOperationObserver
                .RecordFailure(
                activity,
                _logger,
                OperationNames.QueryPage,
                SupportedContainers.CourseAudits,
                startedAt,
                ex);
            throw;
        }
    }
}
