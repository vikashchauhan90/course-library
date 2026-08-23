namespace CourseLibrary.Idp.Application.Abstractions.Serializers;

public interface ISerializer<T>
{
    byte[] Serialize(T value);

    T Deserialize(byte[] data);
}
