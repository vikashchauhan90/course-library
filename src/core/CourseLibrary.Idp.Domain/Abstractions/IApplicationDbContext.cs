using System.Threading;
using System.Threading.Tasks;

namespace CourseLibrary.Idp.Domain.Abstractions;

public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task MigrateAsync(CancellationToken cancellationToken = default);
}
