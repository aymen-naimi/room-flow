using Microsoft.EntityFrameworkCore;
using RoomFlow.Application.Abstractions.Data;

namespace RoomFlow.Infrastructure.Persistence;

public sealed class RoomReadStore : IRoomReadStore
{
    private readonly RoomFlowDbContext _dbContext;

    public RoomReadStore(RoomFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<RoomDto>> GetRoomsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Rooms
            .AsNoTracking()
            .Select(room => new RoomDto(room.Id, room.Name, room.Capacity, room.Location, room.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<RoomDto?> GetRoomByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Rooms
            .AsNoTracking()
            .Where(room => room.Id == id)
            .Select(room => new RoomDto(room.Id, room.Name, room.Capacity, room.Location, room.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
