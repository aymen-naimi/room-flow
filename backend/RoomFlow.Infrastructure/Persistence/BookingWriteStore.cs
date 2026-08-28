using Microsoft.EntityFrameworkCore;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Domain.Entities;

namespace RoomFlow.Infrastructure.Persistence;

public sealed class BookingWriteStore : IBookingWriteStore
{
    private readonly RoomFlowDbContext _dbContext;

    public BookingWriteStore(RoomFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        var room = await _dbContext.Rooms.FindAsync([booking.RoomId], cancellationToken)
            ?? throw new InvalidOperationException($"Room '{booking.RoomId}' was not found.");
        var user = await _dbContext.Users.FindAsync([booking.UserId], cancellationToken)
            ?? throw new InvalidOperationException($"User '{booking.UserId}' was not found.");

        booking.Room = room;
        booking.User = user;
        booking.StartsAt = booking.StartsAt.ToUniversalTime();
        booking.EndsAt = booking.EndsAt.ToUniversalTime();
        _dbContext.Bookings.Add(booking);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var booking = await _dbContext.Bookings.FindAsync([id], cancellationToken);
        if (booking is null)
        {
            return false;
        }

        _dbContext.Bookings.Remove(booking);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
