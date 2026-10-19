using CourseLibrary.Core.Domain.Abstractions.Auditing;
using CourseLibrary.Core.Domain.Abstractions.Entities;
using CourseLibrary.Core.Domain.Abstractions.Security;
using Microsoft.AspNetCore.Identity;

namespace CourseLibrary.Idp.Domain.Entities;

public class ApplicationUser 
    :IdentityUser,
    IEntity<string>,
    IAuditableEntity,
    IConcurrency
{
    public required string FullName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
