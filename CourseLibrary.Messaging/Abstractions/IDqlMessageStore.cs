namespace CourseLibrary.Messaging.Abstractions;

public interface IDqlMessageStore
{
    Task StoreAsync(
        DqlMessage message,
        CancellationToken cancellationToken);
}