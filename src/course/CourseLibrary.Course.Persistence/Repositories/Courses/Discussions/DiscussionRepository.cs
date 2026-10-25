using CourseLibrary.Course.Persistence;
using CourseLibrary.Course.Persistence.Core;
using CourseLibrary.Course.Persistence.Cosmos;
using CourseLibrary.Course.Persistence.Entities;
using CourseLibrary.Course.Persistence.Observability;
using CourseLibrary.Course.Persistence.Repositories.Courses;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;

namespace CourseLibrary.Course.Persistence.Repositories.Courses.Discussions;

internal sealed class DiscussionRepository : IDiscussionRepository
{
    private readonly ICosmosContainerFactory _containerFactory;
    private readonly ILogger<DiscussionRepository> _logger;
    private static readonly CosmosActivitySource ActivitySource = CosmosActivitySource.Shared;

    public DiscussionRepository(
        ICosmosContainerFactory containerFactory,
        ILogger<DiscussionRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(containerFactory);
        ArgumentNullException.ThrowIfNull(logger);
        _containerFactory = containerFactory;
        _logger = logger;
    }

    public async Task<Discussion?> GetByIdAsync(
        string discussionId,
        string courseId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(discussionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(courseId);

        using var activity = ActivitySource.StartActivity(
            CosmosActivityNames.GetItem,
            ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var container = _containerFactory.GetContainer(SupportedContainers.Discussions);
            activity.SetCosmosOperation(
                OperationNames.GetItem,
                SupportedContainers.Discussions,
                container.Database.Id);

            var response = await container.ReadItemAsync<Discussion>(
                discussionId,
                new PartitionKey(CoursePartitionKeyFormats.ForCourse(courseId)),
                cancellationToken: cancellationToken).ConfigureAwait(false);

            CosmosOperationObserver.RecordSuccess(
                activity,
                _logger,
                OperationNames.GetItem,
                SupportedContainers.Discussions,
                startedAt,
                response.RequestCharge);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            CosmosOperationObserver.RecordNotFound(
                activity,
                _logger,
                OperationNames.GetItem,
                SupportedContainers.Discussions,
                startedAt,
                ex.RequestCharge,
                discussionId);
            return null;
        }
        catch (CosmosException ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.GetItem, SupportedContainers.Discussions, startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.GetItem, SupportedContainers.Discussions, startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.GetItem, SupportedContainers.Discussions, startedAt, ex);
            throw;
        }
    }

    public async Task<IReadOnlyList<Discussion>> GetByCourseAsync(
        string courseId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(courseId);

        using var activity = ActivitySource.StartActivity(
            CosmosActivityNames.QueryItems,
            ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var container = _containerFactory.GetContainer(SupportedContainers.Discussions);
            activity.SetCosmosOperation(
                OperationNames.QueryItems,
                SupportedContainers.Discussions,
                container.Database.Id);

            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.courseId = @courseId ORDER BY c.updatedAt DESC")
                .WithParameter("@courseId", courseId);
            var requestOptions = new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(CoursePartitionKeyFormats.ForCourse(courseId))
            };

            using var iterator = container.GetItemQueryIterator<Discussion>(
                query,
                requestOptions: requestOptions);
            var items = new List<Discussion>();
            var requestCharge = 0d;

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);
                items.AddRange(response.Resource);
                requestCharge += response.RequestCharge;
            }

            activity?.SetTag(CosmosActivityTagConstants.ResultCount, items.Count);
            CosmosOperationObserver.RecordSuccess(
                activity,
                _logger,
                OperationNames.QueryItems,
                SupportedContainers.Discussions,
                startedAt,
                requestCharge);
            return items;
        }
        catch (CosmosException ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.QueryItems, SupportedContainers.Discussions, startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.QueryItems, SupportedContainers.Discussions, startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.QueryItems, SupportedContainers.Discussions, startedAt, ex);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(
        string discussionId,
        string courseId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(discussionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(courseId);

        using var activity = ActivitySource.StartActivity(
            CosmosActivityNames.DeleteItem,
            ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var container = _containerFactory.GetContainer(SupportedContainers.Discussions);
            activity.SetCosmosOperation(
                OperationNames.DeleteItem,
                SupportedContainers.Discussions,
                container.Database.Id);

            var response = await container.DeleteItemAsync<Discussion>(
                discussionId,
                new PartitionKey(CoursePartitionKeyFormats.ForCourse(courseId)),
                cancellationToken: cancellationToken).ConfigureAwait(false);
            CosmosOperationObserver.RecordSuccess(
                activity,
                _logger,
                OperationNames.DeleteItem,
                SupportedContainers.Discussions,
                startedAt,
                response.RequestCharge);
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            CosmosOperationObserver.RecordNotFound(
                activity,
                _logger,
                OperationNames.DeleteItem,
                SupportedContainers.Discussions,
                startedAt,
                ex.RequestCharge,
                discussionId);
            return false;
        }
        catch (CosmosException ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.DeleteItem, SupportedContainers.Discussions, startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.DeleteItem, SupportedContainers.Discussions, startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.DeleteItem, SupportedContainers.Discussions, startedAt, ex);
            throw;
        }
    }

    public async Task UpsertAsync(
        Discussion discussion,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(discussion);

        using var activity = ActivitySource.StartActivity(
            CosmosActivityNames.UpsertItem,
            ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var container = _containerFactory.GetContainer(SupportedContainers.Discussions);
            activity.SetCosmosOperation(
                OperationNames.UpsertItem,
                SupportedContainers.Discussions,
                container.Database.Id);

            var response = await container.UpsertItemAsync(
                discussion,
                new PartitionKey(discussion.PartitionKey),
                cancellationToken: cancellationToken).ConfigureAwait(false);
            CosmosOperationObserver.RecordSuccess(
                activity,
                _logger,
                OperationNames.UpsertItem,
                SupportedContainers.Discussions,
                startedAt,
                response.RequestCharge);
        }
        catch (CosmosException ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.UpsertItem, SupportedContainers.Discussions, startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.UpsertItem, SupportedContainers.Discussions, startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.UpsertItem, SupportedContainers.Discussions, startedAt, ex);
            throw;
        }
    }
}
