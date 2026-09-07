using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Domain.Entities;
using CourseLibrary.Models;
using CourseLibrary.Models.Course;

namespace CourseLibrary.Infrastructure.Cosmos;

public sealed class CosmosCourseRepository : ICourseRepository
{
    private readonly ICosmosRepository<Course> _repository;

    public CosmosCourseRepository(ICosmosRepository<Course> repository)
    {
        _repository = repository;
    }

    public async Task<Course?> GetByIdAsync(string courseId, CancellationToken cancellationToken = default)
    {
        var query = new Microsoft.Azure.Cosmos.QueryDefinition(
            "SELECT TOP 1 * FROM c WHERE c.id = @courseId")
            .WithParameter("@courseId", courseId);

        return (await _repository.QueryAsync(query, cancellationToken: cancellationToken))
            .SingleOrDefault();
    }

    public Task<PageResult<Course>> GetByAuthorAsync(
        string authorId,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken = default)
    {
        var query = new Microsoft.Azure.Cosmos.QueryDefinition(
            "SELECT * FROM c WHERE c.authorId = @authorId ORDER BY c.updatedAt DESC")
            .WithParameter("@authorId", authorId);

        return _repository.QueryPageAsync(
            query,
            partitionKey: authorId,
            continuationToken,
            pageSize,
            cancellationToken);
    }

    public Task<PageResult<Course>> SearchAsync(
        CourseSearchCriteria criteria,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken = default)
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

        var queryDefinition = new Microsoft.Azure.Cosmos.QueryDefinition(sql);
        foreach (var parameter in parameters)
            queryDefinition = queryDefinition.WithParameter(parameter.Name, parameter.Value);

        return _repository.QueryPageAsync(
            queryDefinition,
            continuationToken: continuationToken,
            pageSize: pageSize,
            cancellationToken: cancellationToken);
    }

    public Task UpsertAsync(Course course, CancellationToken cancellationToken = default)
        => _repository.UpsertAsync(course, cancellationToken);

    public Task<bool> DeleteAsync(string courseId, string partitionKey, CancellationToken cancellationToken = default)
        => _repository.DeleteAsync(courseId, partitionKey, cancellationToken);
}
