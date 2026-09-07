namespace CourseLibrary.Models.Course;

public sealed record CourseDetails(
    string Id,
    string Title,
    string Description,
    string AuthorId,
    string AuthorName,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? RetiredAt,
    DateTimeOffset? DeletedAt,
    IReadOnlyList<CommentDetails> Comments,
    IReadOnlyList<DiscussionDetails> Discussions);
