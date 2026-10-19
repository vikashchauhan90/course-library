
namespace CourseLibrary.Core.Domain.Abstractions.Security;

public interface IConcurrency
{
    public string? ConcurrencyStamp { get; set; }
}
