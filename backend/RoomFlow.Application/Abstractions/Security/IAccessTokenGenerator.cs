namespace RoomFlow.Application.Abstractions.Security;

public interface IAccessTokenGenerator
{
    string Create(Guid userId, string email);
}
