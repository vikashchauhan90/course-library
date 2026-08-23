using CourseLibrary.Idp.Application.Abstractions.Serializers;

namespace CourseLibrary.Idp.Application.Abstractions.Serialization;

public interface ISerializerFactory
{
    ISerializer<T> Create<T>(SerializerType type);
}