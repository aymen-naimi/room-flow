using RoomFlow.Application.Abstractions.Security;
using RoomFlow.Domain.Enums;

namespace RoomFlow.Application.Tests.Fakes;

internal sealed class FakeAccessTokenGenerator : IAccessTokenGenerator
{
    public string Create(Guid userId, string email, UserRole role) => $"token:{userId}:{email}:{role}";
}
