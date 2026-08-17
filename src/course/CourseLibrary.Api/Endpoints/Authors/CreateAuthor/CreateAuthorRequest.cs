namespace CourseLibrary.Api.Endpoints.Authors;

public sealed record CreateAuthorRequest(string Name, string? Bio, string? Website);