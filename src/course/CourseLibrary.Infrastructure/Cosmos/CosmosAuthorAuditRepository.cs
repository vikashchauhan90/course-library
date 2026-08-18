using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Domain.Entities;

namespace CourseLibrary.Infrastructure.Cosmos;

public sealed class CosmosAuthorAuditRepository(ICosmosRepository<AuthorAuditEntry> repository) : IAuthorAuditRepository
{
    public Task AddAsync(AuthorAuditEntry entry, CancellationToken cancellationToken = default)
        => repository.UpsertAsync(entry, cancellationToken);
}
