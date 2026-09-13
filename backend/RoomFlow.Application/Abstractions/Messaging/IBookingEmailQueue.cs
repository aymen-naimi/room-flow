using RoomFlow.Application.Messaging;

namespace RoomFlow.Application.Abstractions.Messaging;

public interface IBookingEmailQueue
{
    Task PublishAsync(BookingEmailMessage message, CancellationToken cancellationToken = default);
}
