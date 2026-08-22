using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.Infrastructure.Configuration.Messaging;

public sealed class ServiceBusOptions
{
    public const string SectionName = "ServiceBus";

    [Required(AllowEmptyStrings = false)]
    public required string FullyQualifiedNamespace { get; init; }
}
