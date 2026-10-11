namespace CourseLibrary.Platform.Serialization.Abstractions;

public interface ISerializer<T>
{
    byte[] Serialize(T value);

    T Deserialize(byte[] data);
}
