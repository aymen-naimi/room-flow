using RoomFlow.Application.Abstractions.Messaging;
using RoomFlow.Application.Messaging;

namespace RoomFlow.Application.Tests.Fakes;

internal sealed class FakeBookingEmailQueue : IBookingEmailQueue
{
    public IList<BookingEmailMessage> Messages { get; } = new List<BookingEmailMessage>();

    public Exception? PublishException { get; set; }

    public Task PublishAsync(BookingEmailMessage message, CancellationToken cancellationToken = default)
    {
        if (PublishException is not null)
        {
            throw PublishException;
        }

        Messages.Add(message);
        return Task.CompletedTask;
    }
}
