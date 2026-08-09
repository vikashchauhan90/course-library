using Microsoft.AspNetCore.Identity;

namespace CourseLibrary.Idp.Domain.Entities;

public sealed class ApplicationRole : IdentityRole
{
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}
