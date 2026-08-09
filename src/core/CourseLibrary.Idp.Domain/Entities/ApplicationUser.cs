using Microsoft.AspNetCore.Identity;

namespace CourseLibrary.Idp.Domain.Entities;

public sealed class ApplicationUser : IdentityUser
{
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}
