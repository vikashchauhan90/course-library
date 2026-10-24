using CourseLibrary.Course.Persistence.Entities;
using CourseLibrary.Course.Persistence.Observability;
using CourseLibrary.Course.Persistence.Repositories.Abstractions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CourseLibrary.Course.Persistence.Repositories;

internal sealed class CourseAuditRepository
    : CosmosContainerBase,
    ICourseAuditRepository
{
    public CourseAuditRepository(
    CosmosClient client,
    IOptions<CosmosOptions> options,
    ILogger<CourseAuditRepository> logger)
    : base(client, options, "course-audits")
    {
    }
    public Task AddAsync(CourseAuditEntry entry, CancellationToken cancellationToken = default)
        => CourseRepositoryObservability.ExecuteAsync(
            nameof(CourseAuditRepository),
            nameof(AddAsync),
            () => base.UpsertAsync(entry, cancellationToken));
}
