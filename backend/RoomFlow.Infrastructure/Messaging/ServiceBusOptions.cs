namespace RoomFlow.Infrastructure.Messaging;

public sealed class ServiceBusOptions
{
    public const string SectionName = "ServiceBus";

    public string QueueName { get; set; } = "booking-events";

    public string? FullyQualifiedNamespace { get; set; }

    public string? ConnectionString { get; set; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ConnectionString)
        || !string.IsNullOrWhiteSpace(FullyQualifiedNamespace);
}
