using CourseLibrary.Core.Results;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseLibrary.Course.Persistence.Data.Repositories.Abstractions;
using CourseLibrary.Course.Persistence.Observability;
using CourseLibrary.Platform.Persistence.CosmosDB;
using CourseLibrary.Platform.Persistence.CosmosDB.Configurations;
using CourseLibrary.Platform.Persistence.CosmosDB.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CourseLibrary.Course.Persistence.Data.Repositories;

internal sealed class CourseRepository 
    : CosmosRepositoryBase<Domain.Entities.Course, string>,
    ICourseRepository
{
    public CourseRepository(
    CosmosClient client,
    IOptions<CosmosOptions> options,
    ICosmosDocumentConfiguration<Domain.Entities.Course> configuration,
    ILogger<CourseRepository> logger)
    : base(client, options, configuration, logger)
    {
    }

    public async Task<Domain.Entities.Course?> GetByIdAsync(string courseId, CancellationToken cancellationToken = default)
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

    public Task<PageResult<Domain.Entities.Course>> GetByAuthorAsync(
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
                    .WithParameter(
                        "@authorId",
                        Configuration.CreatePartitionKey(authorId));

                return base.QueryPageAsync(
                    query,
                    partitionKey: [authorId],
                    continuationToken,
                    pageSize,
                    cancellationToken);
            });

    public Task<PageResult<Domain.Entities.Course>> SearchAsync(
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
                    parameters.Add((
                        "@authorId",
                        Configuration.CreatePartitionKey(criteria.AuthorId)));
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

    public Task<bool> DeleteAsync(string courseId, string partitionKey, CancellationToken cancellationToken = default)
        => CourseRepositoryObservability.ExecuteAsync(
            nameof(CourseRepository),
            nameof(DeleteAsync),
            () => base.DeleteAsync(courseId, [partitionKey], cancellationToken));

    public new Task UpsertAsync(Domain.Entities.Course course, CancellationToken cancellationToken = default)
        => CourseRepositoryObservability.ExecuteAsync(
            nameof(CourseRepository),
            nameof(UpsertAsync),
            () => base.UpsertAsync(course, cancellationToken));
}
