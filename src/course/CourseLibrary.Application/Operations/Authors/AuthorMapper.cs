using CourseLibrary.Domain.Entities;

namespace CourseLibrary.Application.Operations.Authors;

/// <summary>
/// Mapper for Author domain entity to response models.
/// </summary>
public static class AuthorMapper
{
    /// <summary>
    /// Maps a domain Author entity to an AuthorResponse record.
    /// </summary>
    public static AuthorResponse ToResponse(Author author)
        => new(
            author.Id,
            author.Name,
            author.Bio,
            author.Website,
            author.CreatedAt,
            author.UpdatedAt);

    /// <summary>
    /// Maps a nullable domain Author entity to a nullable AuthorResponse record.
    /// </summary>
    public static AuthorResponse? ToResponse(Author? author)
        => author is null ? null : ToResponse(author);

    /// <summary>
    /// Maps a collection of domain Author entities to AuthorResponse records.
    /// </summary>
    public static IReadOnlyList<AuthorResponse> ToResponses(IReadOnlyList<Author> authors)
        => authors.Select(ToResponse).ToList().AsReadOnly();
}
