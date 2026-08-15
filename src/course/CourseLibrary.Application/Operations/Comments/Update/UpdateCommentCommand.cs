using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Comments.Update;

public sealed record UpdateCommentCommand(string Id, string CourseId, string Content) : ICommand<CourseLibrary.Domain.Entities.Comment>;
