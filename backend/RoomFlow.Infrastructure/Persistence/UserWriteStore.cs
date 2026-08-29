using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Application.Exceptions;
using RoomFlow.Domain.Entities;

namespace RoomFlow.Infrastructure.Persistence;

public sealed class UserWriteStore : IUserWriteStore
{
    private const int SqlServerUniqueIndexViolation = 2601;
    private const int SqlServerUniqueConstraintViolation = 2627;

    private readonly RoomFlowDbContext _dbContext;

    public UserWriteStore(RoomFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsWithEmailAsync(string email, CancellationToken cancellationToken = default)
        => _dbContext.Users.AnyAsync(user => user.Email == email, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Add(user);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueEmailViolation(ex))
        {
            throw new EmailAlreadyTakenException(user.Email);
        }
    }

    private static bool IsUniqueEmailViolation(DbUpdateException exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is SqlException sqlException
                && (sqlException.Number == SqlServerUniqueIndexViolation
                    || sqlException.Number == SqlServerUniqueConstraintViolation))
            {
                return true;
            }
        }

        return false;
    }
}
