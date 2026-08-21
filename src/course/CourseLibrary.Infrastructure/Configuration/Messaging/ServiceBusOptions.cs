using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.Infrastructure.Configuration.Messaging;

public sealed class ServiceBusOptions
{
    [Required(AllowEmptyStrings = false)]
    public const string SectionName = "ServiceBus";
    public required string FullyQualifiedNamespace { get; init; }
    public Dictionary<string, string> Destinations { get; init; } = [];
}
