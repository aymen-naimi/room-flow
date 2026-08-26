using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Domain.Entities;

namespace RoomFlow.Application.Tests.Fakes;

internal sealed class FakeUserReadStore : IUserReadStore
{
    public IList<User> Users { get; } = new List<User>();

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => Task.FromResult(Users.FirstOrDefault(user => user.Email == email));
}
