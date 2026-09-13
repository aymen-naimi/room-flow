using MediatR;
using Microsoft.Extensions.Logging;
using RoomFlow.Application.Abstractions.Concurrency;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Application.Abstractions.Messaging;
using RoomFlow.Application.Exceptions;
using RoomFlow.Application.Messaging;
using RoomFlow.Domain.Entities;

namespace RoomFlow.Application.Features.Bookings.Commands.CreateBooking;

public record CreateBookingCommand(Guid RoomId, Guid UserId, DateTimeOffset StartsAt, DateTimeOffset EndsAt)
    : IRequest<BookingDto>;

public sealed class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, BookingDto>
{
    private readonly IRoomBookingLock _roomLock;
    private readonly IRoomReadStore _roomReadStore;
    private readonly IUserReadStore _userReadStore;
    private readonly IBookingReadStore _bookingReadStore;
    private readonly IBookingWriteStore _bookingWriteStore;
    private readonly IBookingEmailQueue _bookingEmailQueue;
    private readonly ILogger<CreateBookingCommandHandler> _logger;

    public CreateBookingCommandHandler(
        IRoomBookingLock roomLock,
        IRoomReadStore roomReadStore,
        IUserReadStore userReadStore,
        IBookingReadStore bookingReadStore,
        IBookingWriteStore bookingWriteStore,
        IBookingEmailQueue bookingEmailQueue,
        ILogger<CreateBookingCommandHandler> logger)
    {
        _roomLock = roomLock;
        _roomReadStore = roomReadStore;
        _userReadStore = userReadStore;
        _bookingReadStore = bookingReadStore;
        _bookingWriteStore = bookingWriteStore;
        _bookingEmailQueue = bookingEmailQueue;
        _logger = logger;
    }

    public async Task<BookingDto> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        using (await _roomLock.AcquireAsync(request.RoomId, cancellationToken))
        {
            var room = await _roomReadStore.GetRoomByIdAsync(request.RoomId, cancellationToken);
            if (room is null)
            {
                _logger.LogWarning(
                    "Booking creation rejected because room {RoomId} was not found",
                    request.RoomId);
                throw new RoomNotFoundException(request.RoomId);
            }

            var user = await _userReadStore.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                _logger.LogError(
                    "Booking creation failed because the authenticated user was not found");
                throw new InvalidOperationException($"User '{request.UserId}' was not found.");
            }

            var startsAt = request.StartsAt.ToUniversalTime();
            var endsAt = request.EndsAt.ToUniversalTime();

            if (await _bookingReadStore.HasOverlapAsync(request.RoomId, startsAt, endsAt, cancellationToken))
            {
                _logger.LogWarning(
                    "Booking creation rejected because room {RoomId} has an overlapping slot",
                    request.RoomId);
                throw new BookingOverlapException(request.RoomId);
            }

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                RoomId = request.RoomId,
                UserId = request.UserId,
                StartsAt = startsAt,
                EndsAt = endsAt,
                CreatedAt = DateTimeOffset.UtcNow,
                Room = null!,
                User = null!
            };

            await _bookingWriteStore.AddAsync(booking, cancellationToken);

            _logger.LogInformation(
                "Booking created {BookingId} for room {RoomId}",
                booking.Id,
                room.Id);

            await TryPublishEmailAsync(
                BookingEmailEventType.Created,
                booking.Id,
                user.Email,
                $"{user.FirstName} {user.LastName}",
                room.Name,
                booking.StartsAt,
                booking.EndsAt,
                cancellationToken);

            return new BookingDto(
                booking.Id,
                room.Id,
                room.Name,
                user.Id,
                $"{user.FirstName} {user.LastName}",
                booking.StartsAt,
                booking.EndsAt);
        }
    }

    private async Task TryPublishEmailAsync(
        BookingEmailEventType eventType,
        Guid bookingId,
        string userEmail,
        string userDisplayName,
        string roomName,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        CancellationToken cancellationToken)
    {
        try
        {
            await _bookingEmailQueue.PublishAsync(
                new BookingEmailMessage(
                    eventType,
                    bookingId,
                    userEmail,
                    userDisplayName,
                    roomName,
                    startsAt,
                    endsAt),
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to enqueue {EventType} email for booking {BookingId}",
                eventType,
                bookingId);
        }
    }
}
