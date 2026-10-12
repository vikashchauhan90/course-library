namespace CourseLibrary.Platform.Messaging.Abstractions;

public enum DqlMessageStatus
{
    Pending = 0,
    InProgress = 1,
    Replayed = 2,
    ReplayFailed = 3,
    Ignored = 4
}