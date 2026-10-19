namespace CourseLibrary.Platform.Serialization.Abstractions;

public interface ISerializerFactory
{
    ISerializer<T> Create<T>(SerializerType type);
}
