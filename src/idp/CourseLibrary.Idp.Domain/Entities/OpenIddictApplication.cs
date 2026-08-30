using CourseLibrary.Idp.Domain.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

namespace CourseLibrary.Idp.Domain.Entities;

public sealed class OpenIddictApplication
    : OpenIddictEntityFrameworkCoreApplication<
        Guid,
        OpenIddictAuthorization,
        OpenIddictToken>,
    IEntityAudit,
    IEntityConcurrency
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? ConcurrencyStamp { get; set; }
}