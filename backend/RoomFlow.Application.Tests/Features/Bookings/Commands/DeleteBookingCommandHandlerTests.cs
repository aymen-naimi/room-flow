using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Application.Exceptions;
using RoomFlow.Application.Features.Bookings.Commands.DeleteBooking;
using RoomFlow.Application.Tests.Fakes;

namespace RoomFlow.Application.Tests.Features.Bookings.Commands;

public sealed class DeleteBookingCommandHandlerTests
{
    [Fact]
    public async Task Handle_returns_false_when_booking_is_missing()
    {
        var store = new FakeBookingStore();
        var handler = new DeleteBookingCommandHandler(store, store);

        var deleted = await handler.Handle(
            new DeleteBookingCommand(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        Assert.False(deleted);
    }

    [Fact]
    public async Task Handle_throws_when_booking_belongs_to_another_user()
    {
        var store = new FakeBookingStore();
        var ownerId = Guid.NewGuid();
        var booking = Seed(store, ownerId);
        var handler = new DeleteBookingCommandHandler(store, store);

        await Assert.ThrowsAsync<BookingNotOwnedException>(
            () => handler.Handle(new DeleteBookingCommand(booking.Id, Guid.NewGuid()), CancellationToken.None));
        Assert.Single(store.Bookings);
    }

    [Fact]
    public async Task Handle_removes_booking_when_caller_is_owner()
    {
        var store = new FakeBookingStore();
        var ownerId = Guid.NewGuid();
        var booking = Seed(store, ownerId);
        var handler = new DeleteBookingCommandHandler(store, store);

        var deleted = await handler.Handle(new DeleteBookingCommand(booking.Id, ownerId), CancellationToken.None);

        Assert.True(deleted);
        Assert.Empty(store.Bookings);
    }

    private static BookingDto Seed(FakeBookingStore store, Guid userId)
    {
        var booking = new BookingDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Salle A",
            userId,
            "Ada Lovelace",
            new DateTimeOffset(2026, 10, 26, 8, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 26, 9, 0, 0, TimeSpan.Zero));
        store.Bookings.Add(booking);
        return booking;
    }
}
