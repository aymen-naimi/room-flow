using RoomFlow.Application.Abstractions.Security;

namespace RoomFlow.Application.Tests.Fakes;

internal sealed class FakeAccessTokenGenerator : IAccessTokenGenerator
{
    public string Create(Guid userId, string email) => $"token:{userId}:{email}";
}
