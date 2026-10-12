using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Domain.Entities;
using CourseLibrary.Domain.ValueObjects;

namespace CourseLibrary.Course.Persistence.Data;

public sealed class CosmosCourseAuditRepository(ICosmosRepository<CourseAuditEntry, string> repository) : ICourseAuditRepository
{
    public Task AddAsync(CourseAuditEntry entry, CancellationToken cancellationToken = default)
        => repository.UpsertAsync(entry, cancellationToken);
}
