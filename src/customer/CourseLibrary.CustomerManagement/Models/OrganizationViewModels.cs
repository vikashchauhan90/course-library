using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.CustomerManagement.Models;

public sealed class CreateOrganizationViewModel
{
    [Required, StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Guid? ProductPlanId { get; set; }

    public IReadOnlyList<ProductPlanOption> ProductPlans { get; set; } = [];
}

public sealed record ProductPlanOption(Guid Id, string ProductName, string PlanName);

public sealed record OrganizationListItem(Guid Id, string Name, string Role, int MemberCount);

public sealed record OrganizationMemberItem(string Email, string Role);

public sealed record OrganizationSubscriptionItem(string ProductName, string PlanName, string Status);

public sealed class OrganizationDetailsViewModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string CurrentRole { get; init; } = string.Empty;
    public IReadOnlyList<OrganizationMemberItem> Members { get; init; } = [];
    public IReadOnlyList<OrganizationSubscriptionItem> Subscriptions { get; init; } = [];
}
