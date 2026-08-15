using CourseLibrary.Application.Abstractions.Serializers;

namespace CourseLibrary.Application.Abstractions.Serialization;

public interface ISerializerFactory
{
    ISerializer<T> Create<T>(SerializerType type);
}