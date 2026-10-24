using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Course.Persistence.Core;
using CourseLibrary.Course.Persistence.Observability;
using CourseLibrary.Course.Persistence.Repositories.Abstractions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CourseLibrary.Course.Persistence.Repositories;

internal sealed class CourseRepository 
    : CosmosContainerBase,
    ICourseRepository
{
    public CourseRepository(
    CosmosClient client,
    IOptions<CosmosOptions> options,
    ILogger<CourseRepository> logger)
    : base(client, options)
    {
    }

    public async Task<Entities.Course?> GetByIdAsync(string courseId, CancellationToken cancellationToken = default)
    {
        return await CourseRepositoryObservability.ExecuteAsync(
            nameof(CourseRepository),
            nameof(GetByIdAsync),
            () =>
            {
                var query = new QueryDefinition(
                    "SELECT TOP 1 * FROM c WHERE c.id = @courseId")
                    .WithParameter("@courseId", courseId);

                return base.SingleOrDefaultAsync(query, cancellationToken: cancellationToken);
            });

    }

    public Task<PageResult<Entities.Course>> GetByAuthorAsync(
        string authorId,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken = default)
        => CourseRepositoryObservability.ExecuteAsync(
            nameof(CourseRepository),
            nameof(GetByAuthorAsync),
            () =>
            {
                var query = new QueryDefinition(
                    "SELECT * FROM c WHERE c.authorId = @authorId ORDER BY c.updatedAt DESC")
                    .WithParameter("@authorId", authorId);

                return base.QueryPageAsync(
                    query,
                    partitionKey: CoursePartitionKeyFormats.ForAuthor(authorId),
                    continuationToken,
                    pageSize,
                    cancellationToken);
            });

    public Task<PageResult<Entities.Course>> SearchAsync(
        CourseSearchCriteria criteria,
        int pageSize,
        string? continuationToken = null,
        CancellationToken cancellationToken = default)
        => CourseRepositoryObservability.ExecuteAsync(
            nameof(CourseRepository),
            nameof(SearchAsync),
            () =>
            {
                var predicates = new List<string>();
                var sql = "SELECT * FROM c";
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

                if (predicates.Count > 0)
                    sql += " WHERE " + string.Join(" AND ", predicates);

                sql += " ORDER BY c.updatedAt DESC";

                var queryDefinition = new QueryDefinition(sql);
                foreach (var parameter in parameters)
                    queryDefinition = queryDefinition.WithParameter(parameter.Name, parameter.Value);

                return base.QueryPageAsync(
                    queryDefinition,
                    continuationToken: !string.IsNullOrWhiteSpace(continuationToken) ? continuationToken : null,
                    pageSize: pageSize,
                    cancellationToken: cancellationToken);
            });

    public Task<bool> DeleteAsync(string courseId, string authorId, CancellationToken cancellationToken = default)
        => CourseRepositoryObservability.ExecuteAsync(
            nameof(CourseRepository),
            nameof(DeleteAsync),
            () => base.DeleteAsync(courseId, CoursePartitionKeyFormats.ForAuthor(authorId), cancellationToken));

    public new Task UpsertAsync(Entities.Course course, CancellationToken cancellationToken = default)
        => CourseRepositoryObservability.ExecuteAsync(
            nameof(CourseRepository),
            nameof(UpsertAsync),
            () => base.UpsertAsync(course, cancellationToken));
}
