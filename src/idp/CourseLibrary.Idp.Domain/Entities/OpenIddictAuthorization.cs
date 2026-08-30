using CourseLibrary.Idp.Domain.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

namespace CourseLibrary.Idp.Domain.Entities;

public sealed class OpenIddictAuthorization
    : OpenIddictEntityFrameworkCoreAuthorization<
        Guid,
        OpenIddictApplication,
        OpenIddictToken>,
    IEntityAudit,
    IEntityConcurrency
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? ConcurrencyStamp { get; set; }
}