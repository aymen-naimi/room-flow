using MediatR;
using Microsoft.Extensions.Logging;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Application.Abstractions.Messaging;
using RoomFlow.Application.Exceptions;
using RoomFlow.Application.Messaging;

namespace RoomFlow.Application.Features.Bookings.Commands.DeleteBooking;

public record DeleteBookingCommand(Guid Id, Guid UserId) : IRequest<bool>;

public sealed class DeleteBookingCommandHandler : IRequestHandler<DeleteBookingCommand, bool>
{
    private readonly IBookingReadStore _readStore;
    private readonly IBookingWriteStore _writeStore;
    private readonly IUserReadStore _userReadStore;
    private readonly IBookingEmailQueue _bookingEmailQueue;
    private readonly ILogger<DeleteBookingCommandHandler> _logger;

    public DeleteBookingCommandHandler(
        IBookingReadStore readStore,
        IBookingWriteStore writeStore,
        IUserReadStore userReadStore,
        IBookingEmailQueue bookingEmailQueue,
        ILogger<DeleteBookingCommandHandler> logger)
    {
        _readStore = readStore;
        _writeStore = writeStore;
        _userReadStore = userReadStore;
        _bookingEmailQueue = bookingEmailQueue;
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
        if (!deleted)
        {
            return false;
        }

        _logger.LogInformation("Booking deleted {BookingId}", request.Id);

        var user = await _userReadStore.GetByIdAsync(booking.UserId, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning(
                "Skipped cancellation email because user {UserId} was not found for booking {BookingId}",
                booking.UserId,
                booking.Id);
            return true;
        }

        try
        {
            await _bookingEmailQueue.PublishAsync(
                new BookingEmailMessage(
                    BookingEmailEventType.Cancelled,
                    booking.Id,
                    user.Email,
                    booking.UserDisplayName,
                    booking.RoomName,
                    booking.StartsAt,
                    booking.EndsAt),
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to enqueue {EventType} email for booking {BookingId}",
                BookingEmailEventType.Cancelled,
                booking.Id);
        }

        return true;
    }
}
