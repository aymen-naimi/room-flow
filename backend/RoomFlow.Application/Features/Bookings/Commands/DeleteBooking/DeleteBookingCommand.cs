using MediatR;
using RoomFlow.Application.Abstractions.Data;

namespace RoomFlow.Application.Features.Bookings.Commands.DeleteBooking;

public record DeleteBookingCommand(Guid Id, Guid UserId) : IRequest<bool>;

public sealed class DeleteBookingCommandHandler : IRequestHandler<DeleteBookingCommand, bool>
{
    private readonly IBookingReadStore _readStore;
    private readonly IBookingWriteStore _writeStore;

    public DeleteBookingCommandHandler(IBookingReadStore readStore, IBookingWriteStore writeStore)
    {
        _readStore = readStore;
        _writeStore = writeStore;
    }

    public async Task<bool> Handle(DeleteBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _readStore.GetByIdAsync(request.Id, cancellationToken);
        if (booking is null)
        {
            return false;
        }

        if (booking.UserId != request.UserId)
        {
            throw new BookingNotOwnedException(request.Id);
        }

        return await _writeStore.RemoveAsync(request.Id, cancellationToken);
    }
}

public sealed class BookingNotOwnedException : Exception
{
    public BookingNotOwnedException(Guid bookingId)
        : base($"Booking '{bookingId}' belongs to another user.")
    {
    }
}
