using RoomFlow.Api.Contracts.Users;

namespace RoomFlow.Api.Contracts.Auth;

public record LoginResponse(string AccessToken, string RefreshToken, UserResponse User);
