using RoomFlow.Domain.Entities;

namespace RoomFlow.Application.Abstractions.Data;

public interface IBookingWriteStore
{
    Task AddAsync(Booking booking, CancellationToken cancellationToken = default);

    Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default);
}
