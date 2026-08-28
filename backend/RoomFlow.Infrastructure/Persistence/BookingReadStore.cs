using Microsoft.EntityFrameworkCore;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Domain.Entities;

namespace RoomFlow.Infrastructure.Persistence;

public sealed class BookingReadStore : IBookingReadStore
{
    private readonly RoomFlowDbContext _dbContext;

    public BookingReadStore(RoomFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<BookingDto>> GetOverlappingAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        Guid? roomId = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var bookings = Bookings().Where(booking => booking.StartsAt < to && booking.EndsAt > from);
        if (roomId is not null)
        {
            bookings = bookings.Where(booking => booking.RoomId == roomId.Value);
        }

        if (userId is not null)
        {
            bookings = bookings.Where(booking => booking.UserId == userId.Value);
        }

        return await MapToDto(bookings).ToListAsync(cancellationToken);
    }

    public Task<BookingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => MapToDto(Bookings().Where(booking => booking.Id == id))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<bool> HasOverlapAsync(
        Guid roomId,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        CancellationToken cancellationToken = default)
        => Bookings()
            .AnyAsync(
                booking => booking.RoomId == roomId
                    && booking.StartsAt < endsAt
                    && booking.EndsAt > startsAt,
                cancellationToken);

    private IQueryable<Booking> Bookings()
        => _dbContext.Bookings.AsNoTracking();

    private static IQueryable<BookingDto> MapToDto(IQueryable<Booking> bookings)
        => bookings.Select(booking => new BookingDto(
            booking.Id,
            booking.RoomId,
            booking.Room.Name,
            booking.UserId,
            booking.User.FirstName + " " + booking.User.LastName,
            booking.StartsAt,
            booking.EndsAt));
}
