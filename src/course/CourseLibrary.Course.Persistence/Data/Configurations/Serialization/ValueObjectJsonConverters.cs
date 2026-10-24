using System.Text.Json;
namespace CourseLibrary.Course.Persistence.Data.Configurations.Serialization;

internal static class ValueObjectJsonConverters
{
    public static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        return options;
    }

}