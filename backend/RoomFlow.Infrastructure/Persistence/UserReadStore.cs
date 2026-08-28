using Microsoft.EntityFrameworkCore;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Domain.Entities;

namespace RoomFlow.Infrastructure.Persistence;

public sealed class UserReadStore : IUserReadStore
{
    private readonly RoomFlowDbContext _dbContext;

    public UserReadStore(RoomFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
}
