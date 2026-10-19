using OpenTelemetry.Resources;
using System.Runtime.InteropServices;

namespace CourseLibrary.Platform.Observability.Traces.Processors;

public static class EnvironmentResourceBuilderExtensions
{
    public static ResourceBuilder AddCourseLibraryHostAttributes(
        this ResourceBuilder resourceBuilder)
    {
        ArgumentNullException.ThrowIfNull(resourceBuilder);

        return resourceBuilder.AddAttributes(new Dictionary<string, object>
        {
            ["host.name"] = Environment.MachineName,
            ["process.id"] = Environment.ProcessId,
            ["process.runtime"] = RuntimeInformation.FrameworkDescription,
            ["os.description"] = RuntimeInformation.OSDescription,
            ["os.architecture"] = RuntimeInformation.OSArchitecture.ToString(),
            ["process.architecture"] = RuntimeInformation.ProcessArchitecture.ToString()
        });
    }
}