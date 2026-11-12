namespace CourseLibrary.CustomerManagement.Domain.Entities;

public sealed class OrganizationInvitationProduct
{
    public Guid InvitationId { get; set; }
    public Guid ProductId { get; set; }
    public OrganizationInvitation? Invitation { get; set; }
    public Product? Product { get; set; }
}
