using RoomFlow.Application.Abstractions.Security;

namespace RoomFlow.Application.Tests.Fakes;

internal sealed class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password) => $"hashed:{password}";
}
