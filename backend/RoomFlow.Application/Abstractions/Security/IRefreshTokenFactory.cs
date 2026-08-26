namespace RoomFlow.Application.Abstractions.Security;

public record IssuedRefreshToken(string Raw, string Hash, DateTimeOffset ExpiresAt);

public interface IRefreshTokenFactory
{
    IssuedRefreshToken Create();

    string Hash(string raw);
}
