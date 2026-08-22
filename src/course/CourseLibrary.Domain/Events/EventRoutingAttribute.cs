
namespace CourseLibrary.Domain.Events;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class EventRoutingAttribute : Attribute
{
    public string Destination { get; }
    public MessageChannelType Type { get; }

    public EventRoutingAttribute(string destination, MessageChannelType type)
    {
        Destination = destination;
        Type = type;
    }
}