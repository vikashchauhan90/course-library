using CourseLibrary.Course.Persistence;
using CourseLibrary.Course.Persistence.Core;
using CourseLibrary.Course.Persistence.Cosmos;
using CourseLibrary.Course.Persistence.Entities;
using CourseLibrary.Course.Persistence.Observability;
using CourseLibrary.Course.Persistence.Repositories.Courses;
using CourseLibrary.Course.Contracts.DTOs.Courses.Comments;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;

namespace CourseLibrary.Course.Persistence.Repositories.Courses.Comments;

internal sealed class CommentRepository : ICommentRepository
{
    private readonly ICosmosContainerFactory _containerFactory;
    private readonly ILogger<CommentRepository> _logger;
    private static readonly CosmosActivitySource ActivitySource = CosmosActivitySource.Shared;

    public CommentRepository(
        ICosmosContainerFactory containerFactory,
        ILogger<CommentRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(containerFactory);
        ArgumentNullException.ThrowIfNull(logger);
        _containerFactory = containerFactory;
        _logger = logger;
    }

    public async Task<CommentResponse?> GetByIdAsync(
        string commentId,
        string courseId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(courseId);

        using var activity = ActivitySource.StartActivity(
            CosmosActivityNames.GetItem,
            ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var container = _containerFactory.GetContainer(SupportedContainers.Comments);
            activity.SetCosmosOperation(
                OperationNames.GetItem,
                SupportedContainers.Comments,
                container.Database.Id);

            var response = await container.ReadItemAsync<Comment>(
                commentId,
                new PartitionKey(CoursePartitionKeyFormats.ForCourse(courseId)),
                cancellationToken: cancellationToken).ConfigureAwait(false);
            CosmosOperationObserver.RecordSuccess(
                activity,
                _logger,
                OperationNames.GetItem,
                SupportedContainers.Comments,
                startedAt,
                response.RequestCharge);

            return ToResponse(response.Resource);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            CosmosOperationObserver.RecordNotFound(
                activity,
                _logger,
                OperationNames.GetItem,
                SupportedContainers.Comments,
                startedAt,
                ex.RequestCharge,
                commentId);
            return null;
        }
        catch (CosmosException ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.GetItem, SupportedContainers.Comments, startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.GetItem, SupportedContainers.Comments, startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.GetItem, SupportedContainers.Comments, startedAt, ex);
            throw;
        }
    }

    public async Task<IReadOnlyList<CommentResponse>> GetByCourseAsync(
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

            var container = _containerFactory.GetContainer(SupportedContainers.Comments);
            activity.SetCosmosOperation(
                OperationNames.QueryItems,
                SupportedContainers.Comments,
                container.Database.Id);

            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.courseId = @courseId ORDER BY c.createdAt DESC")
                .WithParameter("@courseId", courseId);
            var requestOptions = new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(CoursePartitionKeyFormats.ForCourse(courseId))
            };

            using var iterator = container.GetItemQueryIterator<Comment>(
                query,
                requestOptions: requestOptions);
            var items = new List<Comment>();
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
                SupportedContainers.Comments,
                startedAt,
                requestCharge);
            return items.Select(ToResponse).ToArray();
        }
        catch (CosmosException ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.QueryItems, SupportedContainers.Comments, startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.QueryItems, SupportedContainers.Comments, startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.QueryItems, SupportedContainers.Comments, startedAt, ex);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(
        string commentId,
        string courseId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(courseId);

        using var activity = ActivitySource.StartActivity(
            CosmosActivityNames.DeleteItem,
            ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var container = _containerFactory.GetContainer(SupportedContainers.Comments);
            activity.SetCosmosOperation(
                OperationNames.DeleteItem,
                SupportedContainers.Comments,
                container.Database.Id);

            var response = await container.DeleteItemAsync<Comment>(
                commentId,
                new PartitionKey(CoursePartitionKeyFormats.ForCourse(courseId)),
                cancellationToken: cancellationToken).ConfigureAwait(false);
            CosmosOperationObserver.RecordSuccess(
                activity,
                _logger,
                OperationNames.DeleteItem,
                SupportedContainers.Comments,
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
                SupportedContainers.Comments,
                startedAt,
                ex.RequestCharge,
                commentId);
            return false;
        }
        catch (CosmosException ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.DeleteItem, SupportedContainers.Comments, startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.DeleteItem, SupportedContainers.Comments, startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.DeleteItem, SupportedContainers.Comments, startedAt, ex);
            throw;
        }
    }

    public async Task UpsertAsync(
        CommentResponse comment,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(comment);

        using var activity = ActivitySource.StartActivity(
            CosmosActivityNames.UpsertItem,
            ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var container = _containerFactory.GetContainer(SupportedContainers.Comments);
            activity.SetCosmosOperation(
                OperationNames.UpsertItem,
                SupportedContainers.Comments,
                container.Database.Id);

            var entity = ToEntity(comment);
            var response = await container.UpsertItemAsync(
                entity,
                new PartitionKey(entity.PartitionKey),
                cancellationToken: cancellationToken).ConfigureAwait(false);
            CosmosOperationObserver.RecordSuccess(
                activity,
                _logger,
                OperationNames.UpsertItem,
                SupportedContainers.Comments,
                startedAt,
                response.RequestCharge);
        }
        catch (CosmosException ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.UpsertItem, SupportedContainers.Comments, startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.UpsertItem, SupportedContainers.Comments, startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.UpsertItem, SupportedContainers.Comments, startedAt, ex);
            throw;
        }
    }

    private static CommentResponse ToResponse(Comment comment)
        => new(
            comment.Id,
            comment.CourseId,
            comment.AuthorId,
            comment.Content,
            comment.ParentCommentId,
            comment.CreatedAt,
            comment.UpdatedAt,
            comment.DeletedAt);

    private static Comment ToEntity(CommentResponse comment)
        => new()
        {
            Id = comment.Id,
            CourseId = comment.CourseId,
            PartitionKey = CoursePartitionKeyFormats.ForCourse(comment.CourseId),
            AuthorId = comment.AuthorId,
            Content = comment.Content,
            ParentCommentId = comment.ParentCommentId,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            DeletedAt = comment.DeletedAt
        };
}
