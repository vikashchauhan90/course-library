using CourseLibrary.Core.Domain.Abstractions.Auditing;
using CourseLibrary.Core.Domain.Abstractions.Security;
using OpenIddict.EntityFrameworkCore.Models;

namespace CourseLibrary.Idp.Domain.Entities;

public sealed class OpenIddictToken
    : OpenIddictEntityFrameworkCoreToken<
        Guid,
        OpenIddictApplication,
        OpenIddictAuthorization>,
    IAuditableEntity,
    IConcurrency
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? ConcurrencyStamp { get; set; }
}