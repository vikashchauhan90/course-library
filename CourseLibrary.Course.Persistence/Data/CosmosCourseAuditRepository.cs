using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Domain.Repositories;
using CourseLibrary.Course.Persistence.Abstractions.Repositories;

namespace CourseLibrary.Course.Persistence.Data;

public sealed class CosmosCourseAuditRepository(ICosmosRepository<CourseAuditEntry, string> repository) : ICourseAuditRepository
{
    public Task AddAsync(CourseAuditEntry entry, CancellationToken cancellationToken = default)
        => repository.UpsertAsync(entry, cancellationToken);
}
