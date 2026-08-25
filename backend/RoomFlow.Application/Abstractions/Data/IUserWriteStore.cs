using RoomFlow.Domain.Entities;

namespace RoomFlow.Application.Abstractions.Data;

public interface IUserWriteStore
{
    Task<bool> ExistsWithEmailAsync(string email, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);
}
