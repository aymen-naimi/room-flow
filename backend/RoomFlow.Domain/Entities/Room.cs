namespace RoomFlow.Domain.Entities;

public class Room
{
    public Guid Id { get; init; }
    public required string Name { get; set; }
    public required int Capacity { get; set; }
    public required string Location { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
