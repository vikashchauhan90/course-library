namespace CourseLibrary.Idp.Models.Admin;

public sealed record AdminInvitationsViewModel(
    IReadOnlyList<AdminInvitationItem> Invitations);
