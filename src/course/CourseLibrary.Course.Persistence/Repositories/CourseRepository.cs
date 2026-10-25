using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Course.Contracts.DTOs.Courses;
using CourseLibrary.Course.Persistence;
using CourseLibrary.Course.Persistence.Core;
using CourseLibrary.Course.Persistence.Cosmos;
using CourseLibrary.Course.Persistence.Observability;
using CourseLibrary.Course.Persistence.Repositories.Courses;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.Course.Persistence.Repositories;

internal sealed class CourseRepository : ICourseRepository
{
    private readonly ICosmosContainerFactory _containerFactory;
    private readonly ILogger<CourseRepository> _logger;
    private static readonly CosmosActivitySource ActivitySource = CosmosActivitySource.Shared;

    public CourseRepository(
        ICosmosContainerFactory containerFactory,
        ILogger<CourseRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(containerFactory);
        ArgumentNullException.ThrowIfNull(logger);
        _containerFactory = containerFactory;
        _logger = logger;
    }

    public async Task<Entities.Course?> GetByIdAsync(
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

            var container = _containerFactory.GetContainer(SupportedContainers.Courses);
            activity.SetCosmosOperation(
                OperationNames.QueryItems,
                SupportedContainers.Courses,
                container.Database.Id);

            var query = new QueryDefinition(
                "SELECT TOP 1 * FROM c WHERE c.id = @courseId")
                .WithParameter("@courseId", courseId);

            using var iterator = container.GetItemQueryIterator<Entities.Course>(query);
            var response = await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);
            var course = response.Resource.FirstOrDefault();

            if (course is null)
            {
                CosmosOperationObserver.RecordNotFound(
                    activity,
                    _logger,
                    OperationNames.QueryItems,
                    SupportedContainers.Courses,
                    startedAt,
                    response.RequestCharge,
                    courseId);
                return null;
            }

            activity?.SetTag(CosmosActivityTagConstants.ResultCount, 1);
            CosmosOperationObserver.RecordSuccess(
                activity,
                _logger,
                OperationNames.QueryItems,
                SupportedContainers.Courses,
                startedAt,
                response.RequestCharge);

            return course;
        }
        catch (CosmosException ex)
        {
            CosmosOperationObserver.RecordFailure(
                activity,
                _logger,
                OperationNames.QueryItems,
                SupportedContainers.Courses,
                startedAt,
                ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            CosmosOperationObserver.RecordFailure(
                activity,
                _logger,
                OperationNames.QueryItems,
                SupportedContainers.Courses,
                startedAt,
                ex);
            throw;
        }
        catch (Exception ex)
        {
            CosmosOperationObserver.RecordFailure(
                activity,
                _logger,
                OperationNames.QueryItems,
                SupportedContainers.Courses,
                startedAt,
                ex);
            throw;
        }
    }

    public async Task<PageResult<Entities.Course>> GetByAuthorAsync(
        string authorId,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(authorId);

        using var activity = ActivitySource.StartActivity(
            CosmosActivityNames.QueryPage,
            ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var container = _containerFactory.GetContainer(SupportedContainers.Courses);
            activity.SetCosmosOperation(
                OperationNames.QueryPage,
                SupportedContainers.Courses,
                container.Database.Id);
            SetPageTags(activity, pageSize, continuationToken);

            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.authorId = @authorId ORDER BY c.updatedAt DESC")
                .WithParameter("@authorId", authorId);
            var requestOptions = new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(CoursePartitionKeyFormats.ForAuthor(authorId)),
                MaxItemCount = pageSize
            };

            using var iterator = container.GetItemQueryIterator<Entities.Course>(
                query,
                !string.IsNullOrWhiteSpace(continuationToken) ? continuationToken : null,
                requestOptions);
            var response = await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);
            var items = response.Resource.ToList();
            var nextToken = response.ContinuationToken;
            var hasNextPage = !string.IsNullOrWhiteSpace(nextToken);
            SetResultTags(activity, items.Count, hasNextPage);

            CosmosOperationObserver.RecordSuccess(
                activity,
                _logger,
                OperationNames.QueryPage,
                SupportedContainers.Courses,
                startedAt,
                response.RequestCharge);

            return new PageResult<Entities.Course>(items, nextToken, hasNextPage);
        }
        catch (CosmosException ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.QueryPage, SupportedContainers.Courses, startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.QueryPage, SupportedContainers.Courses, startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.QueryPage, SupportedContainers.Courses, startedAt, ex);
            throw;
        }
    }

    public async Task<PageResult<Entities.Course>> SearchAsync(
        CourseSearchCriteria criteria,
        int pageSize,
        string? continuationToken = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        using var activity = ActivitySource.StartActivity(
            CosmosActivityNames.QueryPage,
            ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var container = _containerFactory.GetContainer(SupportedContainers.Courses);
            activity.SetCosmosOperation(
                OperationNames.QueryPage,
                SupportedContainers.Courses,
                container.Database.Id);
            SetPageTags(activity, pageSize, continuationToken);

            var predicates = new List<string>();
            var parameters = new List<(string Name, object Value)>();

            if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
            {
                predicates.Add("(CONTAINS(c.title, @searchTerm, true) OR CONTAINS(c.description, @searchTerm, true))");
                parameters.Add(("@searchTerm", criteria.SearchTerm));
            }

            if (!string.IsNullOrWhiteSpace(criteria.AuthorId))
            {
                predicates.Add("c.authorId = @authorId");
                parameters.Add(("@authorId", criteria.AuthorId));
            }

            if (!criteria.IncludeDeleted)
                predicates.Add("(NOT IS_DEFINED(c.deletedAt) OR IS_NULL(c.deletedAt))");

            if (!criteria.IncludeRetired)
                predicates.Add("(NOT IS_DEFINED(c.retiredAt) OR IS_NULL(c.retiredAt))");

            var sql = "SELECT * FROM c";
            if (predicates.Count > 0)
                sql += " WHERE " + string.Join(" AND ", predicates);
            sql += " ORDER BY c.updatedAt DESC";

            var query = new QueryDefinition(sql);
            foreach (var parameter in parameters)
                query = query.WithParameter(parameter.Name, parameter.Value);

            var requestOptions = new QueryRequestOptions { MaxItemCount = pageSize };
            if (!string.IsNullOrWhiteSpace(criteria.AuthorId))
            {
                requestOptions.PartitionKey = new PartitionKey(
                    CoursePartitionKeyFormats.ForAuthor(criteria.AuthorId));
            }

            using var iterator = container.GetItemQueryIterator<Entities.Course>(
                query,
                !string.IsNullOrWhiteSpace(continuationToken) ? continuationToken : null,
                requestOptions);
            var response = await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);
            var items = response.Resource.ToList();
            var nextToken = response.ContinuationToken;
            var hasNextPage = !string.IsNullOrWhiteSpace(nextToken);
            SetResultTags(activity, items.Count, hasNextPage);

            CosmosOperationObserver.RecordSuccess(
                activity,
                _logger,
                OperationNames.QueryPage,
                SupportedContainers.Courses,
                startedAt,
                response.RequestCharge);

            return new PageResult<Entities.Course>(items, nextToken, hasNextPage);
        }
        catch (CosmosException ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.QueryPage, SupportedContainers.Courses, startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.QueryPage, SupportedContainers.Courses, startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.QueryPage, SupportedContainers.Courses, startedAt, ex);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(
        string courseId,
        string authorId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(courseId);
        ArgumentException.ThrowIfNullOrWhiteSpace(authorId);

        using var activity = ActivitySource.StartActivity(
            CosmosActivityNames.DeleteItem,
            ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var container = _containerFactory.GetContainer(SupportedContainers.Courses);
            activity.SetCosmosOperation(
                OperationNames.DeleteItem,
                SupportedContainers.Courses,
                container.Database.Id);

            var response = await container.DeleteItemAsync<Entities.Course>(
                courseId,
                new PartitionKey(CoursePartitionKeyFormats.ForAuthor(authorId)),
                cancellationToken: cancellationToken).ConfigureAwait(false);
            CosmosOperationObserver.RecordSuccess(
                activity,
                _logger,
                OperationNames.DeleteItem,
                SupportedContainers.Courses,
                startedAt,
                response.RequestCharge);

            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            CosmosOperationObserver.RecordNotFound(
                activity,
                _logger,
                OperationNames.DeleteItem,
                SupportedContainers.Courses,
                startedAt,
                ex.RequestCharge,
                courseId);
            return false;
        }
        catch (CosmosException ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.DeleteItem, SupportedContainers.Courses, startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.DeleteItem, SupportedContainers.Courses, startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.DeleteItem, SupportedContainers.Courses, startedAt, ex);
            throw;
        }
    }

    public async Task UpsertAsync(
        Entities.Course course,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(course);

        using var activity = ActivitySource.StartActivity(
            CosmosActivityNames.UpsertItem,
            ActivityKind.Client);
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var container = _containerFactory.GetContainer(SupportedContainers.Courses);
            activity.SetCosmosOperation(
                OperationNames.UpsertItem,
                SupportedContainers.Courses,
                container.Database.Id);

            var response = await container.UpsertItemAsync(
                course,
                new PartitionKey(course.PartitionKey),
                cancellationToken: cancellationToken).ConfigureAwait(false);
            CosmosOperationObserver.RecordSuccess(
                activity,
                _logger,
                OperationNames.UpsertItem,
                SupportedContainers.Courses,
                startedAt,
                response.RequestCharge);
        }
        catch (CosmosException ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.UpsertItem, SupportedContainers.Courses, startedAt, ex);
            throw ex.ToApplicationException();
        }
        catch (OperationCanceledException ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.UpsertItem, SupportedContainers.Courses, startedAt, ex);
            throw;
        }
        catch (Exception ex)
        {
            CosmosOperationObserver.RecordFailure(activity, _logger, OperationNames.UpsertItem, SupportedContainers.Courses, startedAt, ex);
            throw;
        }
    }

    private static void SetPageTags(
        Activity? activity,
        int pageSize,
        string? continuationToken)
    {
        activity?.SetTag(CosmosActivityTagConstants.PageSize, pageSize);
        activity?.SetTag(
            CosmosActivityTagConstants.HasContinuationToken,
            !string.IsNullOrWhiteSpace(continuationToken));
    }

    private static void SetResultTags(Activity? activity, int resultCount, bool hasNextPage)
    {
        activity?.SetTag(CosmosActivityTagConstants.ResultCount, resultCount);
        activity?.SetTag(CosmosActivityTagConstants.HasNextPage, hasNextPage);
    }
}
