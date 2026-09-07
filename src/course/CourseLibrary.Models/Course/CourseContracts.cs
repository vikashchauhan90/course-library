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

public sealed record CommentDetails(
    string Id,
    string CourseId,
    string AuthorId,
    string Content,
    string? ParentCommentId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record DiscussionDetails(
    string Id,
    string CourseId,
    string Title,
    string Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CourseWriteRequest(string Title, string Description);
