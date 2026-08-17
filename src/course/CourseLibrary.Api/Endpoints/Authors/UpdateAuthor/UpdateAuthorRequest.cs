namespace CourseLibrary.Api.Endpoints.Authors.UpdateAuthor;

public sealed record UpdateAuthorRequest(string Name, string? Bio, string? Website);