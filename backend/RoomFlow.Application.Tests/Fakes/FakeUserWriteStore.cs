using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Domain.Entities;

namespace RoomFlow.Application.Tests.Fakes;

internal sealed class FakeUserWriteStore : IUserWriteStore
{
    public IList<User> Users { get; } = new List<User>();

    public Task<bool> ExistsWithEmailAsync(string email, CancellationToken cancellationToken = default)
        => Task.FromResult(Users.Any(user => user.Email == email));

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        Users.Add(user);
        return Task.CompletedTask;
    }
}
