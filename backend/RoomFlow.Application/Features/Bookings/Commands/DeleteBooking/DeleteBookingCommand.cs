using MediatR;
using Microsoft.Extensions.Logging;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Application.Exceptions;

namespace RoomFlow.Application.Features.Bookings.Commands.DeleteBooking;

public record DeleteBookingCommand(Guid Id, Guid UserId) : IRequest<bool>;

public sealed class DeleteBookingCommandHandler : IRequestHandler<DeleteBookingCommand, bool>
{
    private readonly IBookingReadStore _readStore;
    private readonly IBookingWriteStore _writeStore;
    private readonly ILogger<DeleteBookingCommandHandler> _logger;

    public DeleteBookingCommandHandler(
        IBookingReadStore readStore,
        IBookingWriteStore writeStore,
        ILogger<DeleteBookingCommandHandler> logger)
    {
        _readStore = readStore;
        _writeStore = writeStore;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _readStore.GetByIdAsync(request.Id, cancellationToken);
        if (booking is null)
        {
            _logger.LogWarning(
                "Booking deletion skipped because booking {BookingId} was not found",
                request.Id);
            return false;
        }

        if (booking.UserId != request.UserId)
        {
            _logger.LogWarning(
                "Booking deletion rejected because booking {BookingId} belongs to another user",
                request.Id);
            throw new BookingNotOwnedException(request.Id);
        }

        var deleted = await _writeStore.RemoveAsync(request.Id, cancellationToken);
        if (deleted)
        {
            _logger.LogInformation("Booking deleted {BookingId}", request.Id);
        }

        return deleted;
    }
}
