using CourseLibrary.Domain.Abstractions;
using Microsoft.Azure.Cosmos;

namespace CourseLibrary.Infrastructure.Cosmos;

public interface ICosmosRepository<T> where T : ICosmosPartitioned
{
    Task<T?> GetByIdAsync(string id, string partitionKey, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> QueryAsync(string query, string partitionKey, QueryRequestOptions? requestOptions = null, CancellationToken cancellationToken = default);
    Task UpsertAsync(T item, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, string partitionKey, CancellationToken cancellationToken = default);
}
