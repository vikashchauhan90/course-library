namespace CourseLibrary.Course.Domain.Entities;

public interface IPartitionedEntity
{
    string PartitionKey { get; }
}
