using CourseLibrary.Idp.Domain.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace CourseLibrary.Idp.Domain.Entities;

public class ApplicationRole : IdentityRole, IEntity<string>, IEntityAudit
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
