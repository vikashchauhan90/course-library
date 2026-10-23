namespace CourseLibrary.Platform.Observability.Logs.Processors;

public interface IPrefixProvider
{
    public string Prefix { get; }
}
