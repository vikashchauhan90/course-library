using CourseLibrary.Idp.Domain.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

namespace CourseLibrary.Idp.Domain.Entities;

public sealed class OpenIddictToken
    : OpenIddictEntityFrameworkCoreToken<
        Guid,
        OpenIddictApplication,
        OpenIddictAuthorization>,
    IEntityAudit,
    IEntityConcurrency
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? ConcurrencyStamp { get; set; }
}