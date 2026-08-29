using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Application.Exceptions;
using RoomFlow.Domain.Entities;

namespace RoomFlow.Infrastructure.Persistence;

public sealed class RoomWriteStore : IRoomWriteStore
{
    private const int SqlServerUniqueIndexViolation = 2601;

    private readonly RoomFlowDbContext _dbContext;

    public RoomWriteStore(RoomFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsWithNameAsync(string name, CancellationToken cancellationToken = default)
        => _dbContext.Rooms.AnyAsync(room => room.Name == name, cancellationToken);

    public async Task AddAsync(Room room, CancellationToken cancellationToken = default)
    {
        _dbContext.Rooms.Add(room);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueNameViolation(ex))
        {
            throw new RoomNameAlreadyTakenException(room.Name);
        }
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var room = await _dbContext.Rooms.FindAsync([id], cancellationToken);
        if (room is null)
        {
            return false;
        }

        _dbContext.Rooms.Remove(room);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static bool IsUniqueNameViolation(DbUpdateException exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is SqlException sqlException
                && sqlException.Number == SqlServerUniqueIndexViolation)
            {
                return true;
            }
        }

        return false;
    }
}
