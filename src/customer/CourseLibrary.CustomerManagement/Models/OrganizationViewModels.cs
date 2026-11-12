using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.CustomerManagement.Models;

public sealed class CreateOrganizationViewModel
{
    [Required, StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Guid? ProductPlanId { get; set; }

    public bool GiveCreatorProductAccess { get; set; } = true;
    public IReadOnlyList<ProductPlanOption> ProductPlans { get; set; } = [];
}

public sealed record ProductPlanOption(Guid Id, string ProductName, string PlanName);

public sealed record OrganizationListItem(Guid Id, string Name, string Role, int MemberCount);

public sealed record OrganizationMemberItem(string UserId, string Email, string Role, IReadOnlyList<string> Products);

public sealed record OrganizationSubscriptionItem(string ProductName, string PlanName, string Status);
public sealed record ProductAccessOption(Guid ProductId, string ProductName, string PlanName);
public sealed record OrganizationMemberOption(string UserId, string Email);

public sealed class OrganizationDetailsViewModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string CurrentRole { get; init; } = string.Empty;
    public bool CanManage { get; init; }
    public IReadOnlyList<OrganizationMemberItem> Members { get; init; } = [];
    public IReadOnlyList<OrganizationSubscriptionItem> Subscriptions { get; init; } = [];
}

public sealed class AddOrganizationProductViewModel
{
    public Guid OrganizationId { get; set; }
    public Guid? ProductPlanId { get; set; }
    public List<string> SelectedUserIds { get; set; } = [];
    public IReadOnlyList<ProductPlanOption> ProductPlans { get; set; } = [];
    public IReadOnlyList<OrganizationMemberOption> Members { get; set; } = [];
}

public sealed class ManageMemberProductAccessViewModel
{
    public Guid OrganizationId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<Guid> SelectedProductIds { get; set; } = [];
    public IReadOnlyList<ProductAccessOption> AvailableProducts { get; set; } = [];
}
