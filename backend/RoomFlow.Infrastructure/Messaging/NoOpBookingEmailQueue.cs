using Microsoft.Extensions.Logging;
using RoomFlow.Application.Abstractions.Messaging;
using RoomFlow.Application.Messaging;

namespace RoomFlow.Infrastructure.Messaging;

public sealed class NoOpBookingEmailQueue : IBookingEmailQueue
{
    private readonly ILogger<NoOpBookingEmailQueue> _logger;

    public NoOpBookingEmailQueue(ILogger<NoOpBookingEmailQueue> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync(BookingEmailMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Service Bus is not configured; skipped {EventType} email for booking {BookingId}",
            message.EventType,
            message.BookingId);
        return Task.CompletedTask;
    }
}
