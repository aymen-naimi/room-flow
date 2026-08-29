namespace RoomFlow.Domain.Entities;

public class User
{
    public Guid Id { get; init; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastSignIn { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Room> CreatedRooms { get; set; } = new List<Room>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
