namespace CourseLibrary.Idp.Models.Admin;

public sealed record AdminClientsViewModel(
    IReadOnlyList<AdminClientItem> Clients);