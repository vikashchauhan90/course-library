using CourseLibrary.Idp.Domain.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace CourseLibrary.Idp.Domain.Entities;

public class ApplicationUser : IdentityUser, IEntity<string>, IEntityAudit, IEntityConcurrency
{
    public required string FullName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
