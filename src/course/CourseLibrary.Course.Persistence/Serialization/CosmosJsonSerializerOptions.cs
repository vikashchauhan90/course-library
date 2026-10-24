using System.Text.Json;

namespace CourseLibrary.Course.Persistence.Serialization;

internal static class CosmosJsonSerializerOptions
{
    public static JsonSerializerOptions CreateOptions()
    {
        return new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
    }
}