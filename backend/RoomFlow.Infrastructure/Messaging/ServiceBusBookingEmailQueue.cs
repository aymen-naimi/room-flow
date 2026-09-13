using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RoomFlow.Application.Abstractions.Messaging;
using RoomFlow.Application.Messaging;

namespace RoomFlow.Infrastructure.Messaging;

public sealed class ServiceBusBookingEmailQueue : IBookingEmailQueue, IAsyncDisposable
{
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusBookingEmailQueue> _logger;

    public ServiceBusBookingEmailQueue(
        ServiceBusClient client,
        IOptions<ServiceBusOptions> options,
        ILogger<ServiceBusBookingEmailQueue> logger)
    {
        _sender = client.CreateSender(options.Value.QueueName);
        _logger = logger;
    }

    public async Task PublishAsync(BookingEmailMessage message, CancellationToken cancellationToken = default)
    {
        var body = JsonSerializer.Serialize(message, BookingEmailJson.Options);
        var busMessage = new ServiceBusMessage(body)
        {
            ContentType = "application/json",
            MessageId = $"{message.BookingId}:{message.EventType}"
        };
        busMessage.ApplicationProperties["eventType"] = message.EventType.ToString();

        await _sender.SendMessageAsync(busMessage, cancellationToken);
        _logger.LogInformation(
            "Enqueued {EventType} booking email {BookingId}",
            message.EventType,
            message.BookingId);
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
    }
}
