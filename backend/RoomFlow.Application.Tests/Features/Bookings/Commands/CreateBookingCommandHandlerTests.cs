using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Application.Concurrency;
using RoomFlow.Application.Exceptions;
using RoomFlow.Application.Features.Bookings.Commands.CreateBooking;
using RoomFlow.Application.Tests.Fakes;
using RoomFlow.Domain.Entities;

namespace RoomFlow.Application.Tests.Features.Bookings.Commands;

public sealed class CreateBookingCommandHandlerTests
{
    private static readonly DateTimeOffset StartsAt = new(2026, 10, 26, 8, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset EndsAt = new(2026, 10, 26, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_adds_booking_when_slot_is_free()
    {
        var fixture = CreateFixture();
        var command = new CreateBookingCommand(fixture.Room.Id, fixture.User.Id, StartsAt, EndsAt);

        var result = await fixture.Handler.Handle(command, CancellationToken.None);

        Assert.Equal(fixture.Room.Id, result.RoomId);
        Assert.Equal(fixture.Room.Name, result.RoomName);
        Assert.Equal(fixture.User.Id, result.UserId);
        Assert.Equal("Ada Lovelace", result.UserDisplayName);
        Assert.Equal(StartsAt, result.StartsAt);
        Assert.Equal(EndsAt, result.EndsAt);
        Assert.Single(fixture.Bookings.Bookings);
    }

    [Fact]
    public async Task Handle_throws_when_room_is_missing()
    {
        var fixture = CreateFixture();
        var command = new CreateBookingCommand(Guid.NewGuid(), fixture.User.Id, StartsAt, EndsAt);

        await Assert.ThrowsAsync<RoomNotFoundException>(
            () => fixture.Handler.Handle(command, CancellationToken.None));
        Assert.Empty(fixture.Bookings.Bookings);
    }

    [Fact]
    public async Task Handle_throws_when_slot_overlaps_sequentially()
    {
        var fixture = CreateFixture();
        await fixture.Handler.Handle(
            new CreateBookingCommand(fixture.Room.Id, fixture.User.Id, StartsAt, EndsAt),
            CancellationToken.None);

        var overlapping = new CreateBookingCommand(
            fixture.Room.Id,
            fixture.User.Id,
            new DateTimeOffset(2026, 10, 26, 8, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 26, 9, 30, 0, TimeSpan.Zero));

        await Assert.ThrowsAsync<BookingOverlapException>(
            () => fixture.Handler.Handle(overlapping, CancellationToken.None));
        Assert.Single(fixture.Bookings.Bookings);
    }

    [Fact]
    public async Task Handle_allows_adjacent_slots_on_the_same_room()
    {
        var fixture = CreateFixture();
        await fixture.Handler.Handle(
            new CreateBookingCommand(fixture.Room.Id, fixture.User.Id, StartsAt, EndsAt),
            CancellationToken.None);

        var adjacent = new CreateBookingCommand(
            fixture.Room.Id,
            fixture.User.Id,
            EndsAt,
            new DateTimeOffset(2026, 10, 26, 10, 0, 0, TimeSpan.Zero));

        var result = await fixture.Handler.Handle(adjacent, CancellationToken.None);

        Assert.Equal(EndsAt, result.StartsAt);
        Assert.Equal(2, fixture.Bookings.Bookings.Count);
    }

    [Fact]
    public async Task Handle_detects_overlap_across_dst_transition()
    {
        var fixture = CreateFixture();
        var existingStart = new DateTimeOffset(2026, 10, 24, 23, 0, 0, TimeSpan.Zero);
        var existingEnd = new DateTimeOffset(2026, 10, 25, 1, 0, 0, TimeSpan.Zero);
        await fixture.Handler.Handle(
            new CreateBookingCommand(fixture.Room.Id, fixture.User.Id, existingStart, existingEnd),
            CancellationToken.None);

        var overlapping = new CreateBookingCommand(
            fixture.Room.Id,
            fixture.User.Id,
            new DateTimeOffset(2026, 10, 25, 0, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 25, 2, 0, 0, TimeSpan.Zero));

        await Assert.ThrowsAsync<BookingOverlapException>(
            () => fixture.Handler.Handle(overlapping, CancellationToken.None));
        Assert.Single(fixture.Bookings.Bookings);
    }

    [Fact]
    public async Task Handle_rejects_one_of_two_parallel_overlaps_on_the_same_room()
    {
        var fixture = CreateFixture();
        fixture.Bookings.OverlapCheckDelay = TimeSpan.FromMilliseconds(80);
        var first = new CreateBookingCommand(fixture.Room.Id, fixture.User.Id, StartsAt, EndsAt);
        var second = new CreateBookingCommand(
            fixture.Room.Id,
            fixture.User.Id,
            new DateTimeOffset(2026, 10, 26, 8, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 26, 9, 30, 0, TimeSpan.Zero));

        var results = await Task.WhenAll(
            TryCreate(fixture.Handler, first),
            TryCreate(fixture.Handler, second));

        Assert.Single(results, static result => result.Dto is not null);
        Assert.Single(results, static result => result.Error is BookingOverlapException);
        Assert.Single(fixture.Bookings.Bookings);
    }

    [Fact]
    public async Task Handle_allows_parallel_creates_on_different_rooms()
    {
        var fixture = CreateFixture();
        var otherRoom = new RoomDto(Guid.NewGuid(), "Salle B", 6, "RDC", DateTimeOffset.UtcNow);
        fixture.Rooms.Rooms.Add(otherRoom);
        fixture.Bookings.OverlapCheckDelay = TimeSpan.FromMilliseconds(80);

        var results = await Task.WhenAll(
            fixture.Handler.Handle(
                new CreateBookingCommand(fixture.Room.Id, fixture.User.Id, StartsAt, EndsAt),
                CancellationToken.None),
            fixture.Handler.Handle(
                new CreateBookingCommand(otherRoom.Id, fixture.User.Id, StartsAt, EndsAt),
                CancellationToken.None));

        Assert.Equal(2, results.Length);
        Assert.Equal(2, fixture.Bookings.Bookings.Count);
        Assert.Equal(2, fixture.Bookings.Bookings.Select(booking => booking.RoomId).Distinct().Count());
    }

    private static async Task<(BookingDto? Dto, Exception? Error)> TryCreate(
        CreateBookingCommandHandler handler,
        CreateBookingCommand command)
    {
        try
        {
            return (await handler.Handle(command, CancellationToken.None), null);
        }
        catch (Exception exception)
        {
            return (null, exception);
        }
    }

    private static Fixture CreateFixture()
    {
        var rooms = new FakeRoomReadStore();
        var users = new FakeUserReadStore();
        var bookings = new FakeBookingStore();
        var room = new RoomDto(Guid.NewGuid(), "Salle A", 8, "RDC", DateTimeOffset.UtcNow);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "ada@example.com",
            PasswordHash = "hash",
            FirstName = "Ada",
            LastName = "Lovelace",
            CreatedAt = DateTimeOffset.UtcNow
        };
        rooms.Rooms.Add(room);
        users.Users.Add(user);

        var handler = new CreateBookingCommandHandler(
            new RoomBookingLock(),
            rooms,
            users,
            bookings,
            bookings);

        return new Fixture(handler, bookings, rooms, user, room);
    }

    private sealed record Fixture(
        CreateBookingCommandHandler Handler,
        FakeBookingStore Bookings,
        FakeRoomReadStore Rooms,
        User User,
        RoomDto Room);
}
