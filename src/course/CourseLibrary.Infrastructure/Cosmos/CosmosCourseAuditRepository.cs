using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Domain.Entities;

namespace CourseLibrary.Infrastructure.Cosmos;

public sealed class CosmosCourseAuditRepository(ICosmosRepository<CourseAuditEntry> repository) : ICourseAuditRepository
{
    public Task AddAsync(CourseAuditEntry entry, CancellationToken cancellationToken = default)
        => repository.UpsertAsync(entry, cancellationToken);
}
