namespace CourseLibrary.Idp.Domain.Abstractions;

public interface IEntityAudit
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

}
