using CourseLibrary.Course.Domain.Entities;

namespace CourseLibrary.Course.Persistence.Data.Repositories;

public sealed class CosmosCourseAuditRepository(ICosmosRepository<CourseAuditEntry, string> repository) : ICourseAuditRepository
{
    public Task AddAsync(CourseAuditEntry entry, CancellationToken cancellationToken = default)
        => repository.UpsertAsync(entry, cancellationToken);
}
