using CourseLibrary.Domain.Events;

namespace CourseLibrary.Application.Abstractions.Messaging;

public interface IDqlMessageStore
{
    Task StoreAsync(
        DqlMessage message,
        CancellationToken cancellationToken);
}