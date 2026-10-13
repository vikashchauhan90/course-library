namespace CourseLibrary.Course.Client.Security;

public interface ICommonHeadersProvider
{
    IReadOnlyDictionary<string, string> GetHeaders();
}
