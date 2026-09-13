namespace CourseLibrary.Domain.Abstractions;

public interface IEntity<T> where T : notnull
{
    public T Id { get; }
}