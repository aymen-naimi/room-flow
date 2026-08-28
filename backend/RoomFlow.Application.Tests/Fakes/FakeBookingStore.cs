using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Domain.Entities;

namespace RoomFlow.Application.Tests.Fakes;

internal sealed class FakeBookingStore : IBookingReadStore, IBookingWriteStore
{
    private readonly object _sync = new();

    public List<BookingDto> Bookings { get; } = [];

    public TimeSpan OverlapCheckDelay { get; set; }

    public async Task<IReadOnlyList<BookingDto>> GetOverlappingAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        Guid? roomId = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        lock (_sync)
        {
            return Bookings
                .Where(booking => booking.StartsAt < to && booking.EndsAt > from)
                .Where(booking => roomId is null || booking.RoomId == roomId)
                .Where(booking => userId is null || booking.UserId == userId)
                .ToList();
        }
    }

    public async Task<BookingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        lock (_sync)
        {
            return Bookings.FirstOrDefault(booking => booking.Id == id);
        }
    }

    public async Task<bool> HasOverlapAsync(
        Guid roomId,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        CancellationToken cancellationToken = default)
    {
        if (OverlapCheckDelay > TimeSpan.Zero)
        {
            await Task.Delay(OverlapCheckDelay, cancellationToken);
        }

        lock (_sync)
        {
            return Bookings.Any(booking =>
                booking.RoomId == roomId
                && booking.StartsAt < endsAt
                && booking.EndsAt > startsAt);
        }
    }

    public async Task AddAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        lock (_sync)
        {
            Bookings.Add(new BookingDto(
                booking.Id,
                booking.RoomId,
                "Salle",
                booking.UserId,
                "User",
                booking.StartsAt.ToUniversalTime(),
                booking.EndsAt.ToUniversalTime()));
        }
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        lock (_sync)
        {
            var booking = Bookings.FirstOrDefault(item => item.Id == id);
            if (booking is null)
            {
                return false;
            }

            Bookings.Remove(booking);
            return true;
        }
    }
}
