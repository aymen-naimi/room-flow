using RoomFlow.Domain.Entities;

namespace RoomFlow.Application.Abstractions.Data;

public interface IUserReadStore
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
