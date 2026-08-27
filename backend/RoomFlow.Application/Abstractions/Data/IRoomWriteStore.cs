using RoomFlow.Domain.Entities;

namespace RoomFlow.Application.Abstractions.Data;

public interface IRoomWriteStore
{
    Task<bool> ExistsWithNameAsync(string name, CancellationToken cancellationToken = default);

    Task AddAsync(Room room, CancellationToken cancellationToken = default);

    Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default);
}
