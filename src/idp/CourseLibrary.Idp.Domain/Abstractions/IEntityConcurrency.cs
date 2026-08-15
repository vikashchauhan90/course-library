
namespace CourseLibrary.Idp.Domain.Abstractions;

public interface IEntityConcurrency
{
    public string? ConcurrencyStamp { get; set; }
}
