namespace RoomFlow.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required string TokenHash { get; set; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public DateTimeOffset? RevokedAt { get; set; }
    public User User { get; set; } = null!;
}
