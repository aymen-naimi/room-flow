namespace RoomFlow.Domain.Entities;

public class Booking
{
    public Guid Id { get; init; }
    public required Guid RoomId { get; set; }
    public required Guid UserId { get; set; }
    public required DateTimeOffset StartsAt { get; set; }
    public required DateTimeOffset EndsAt { get; set; }
    public required Room Room { get; set; }
    public required User User { get; set; }
}
