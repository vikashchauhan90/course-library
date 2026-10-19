using System.ComponentModel.DataAnnotations;


namespace CourseLibrary.Platform.Messaging.Configurations;

public sealed class ServiceBusOptions
{
    public const string SectionName = "ServiceBus";

    [Required(AllowEmptyStrings = false)]
    public required string ConnectionString { get; init; }
}
