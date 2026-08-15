namespace CourseLibrary.Domain.Abstractions;

public interface ICosmosPartitioned
{
    string PartitionKeyValue { get; }
}
