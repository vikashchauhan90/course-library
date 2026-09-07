namespace CourseLibrary.Client.Observability;

public interface ICommonHeadersProvider
{
    IReadOnlyDictionary<string, string> GetHeaders();
}
