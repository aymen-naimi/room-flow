using MediatR;
using RoomFlow.Application.Abstractions.Concurrency;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Application.Exceptions;
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

    public CreateBookingCommandHandler(
        IRoomBookingLock roomLock,
        IRoomReadStore roomReadStore,
        IUserReadStore userReadStore,
        IBookingReadStore bookingReadStore,
        IBookingWriteStore bookingWriteStore)
    {
        _roomLock = roomLock;
        _roomReadStore = roomReadStore;
        _userReadStore = userReadStore;
        _bookingReadStore = bookingReadStore;
        _bookingWriteStore = bookingWriteStore;
    }

    public async Task<BookingDto> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        using (await _roomLock.AcquireAsync(request.RoomId, cancellationToken))
        {
            var room = await _roomReadStore.GetRoomByIdAsync(request.RoomId, cancellationToken);
            if (room is null)
            {
                throw new RoomNotFoundException(request.RoomId);
            }

            var user = await _userReadStore.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                throw new InvalidOperationException($"User '{request.UserId}' was not found.");
            }

            var startsAt = request.StartsAt.ToUniversalTime();
            var endsAt = request.EndsAt.ToUniversalTime();

            if (await _bookingReadStore.HasOverlapAsync(request.RoomId, startsAt, endsAt, cancellationToken))
            {
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
}
