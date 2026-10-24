namespace CourseLibrary.Platform.Observability.Logs.Processors;

internal class StaticPrefixProvider : IPrefixProvider
{
    public StaticPrefixProvider(string prefix)
    {
        Prefix = prefix;
    }
    public string Prefix { get; }

}
