using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Platform.Persistence.CosmosDB;
using CourseLibrary.Platform.Persistence.CosmosDB.Configurations;
using CourseLibrary.Platform.Persistence.CosmosDB.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CourseLibrary.Course.Persistence.Data.Repositories;

public sealed class CourseAuditRepository
    : CosmosRepositoryBase<CourseAuditEntry, string>,
    ICourseAuditRepository
{
    public CourseAuditRepository(
    CosmosClient client,
    IOptions<CosmosOptions> options,
    ICosmosDocumentConfiguration<CourseAuditEntry> configuration,
    ILogger<CourseAuditRepository> logger)
    : base(client, options, configuration, logger)
    {
    }
    public Task AddAsync(CourseAuditEntry entry, CancellationToken cancellationToken = default)
        => base.UpsertAsync(entry, cancellationToken);
}
