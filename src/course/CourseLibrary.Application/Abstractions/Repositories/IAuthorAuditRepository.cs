using CourseLibrary.Domain.Entities;

namespace CourseLibrary.Application.Abstractions.Repositories;

public interface IAuthorAuditRepository
{
    Task AddAsync(AuthorAuditEntry entry, CancellationToken cancellationToken = default);
}
