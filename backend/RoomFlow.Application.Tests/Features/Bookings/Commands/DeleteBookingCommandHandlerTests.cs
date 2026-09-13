using Microsoft.Extensions.Logging.Abstractions;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Application.Exceptions;
using RoomFlow.Application.Features.Bookings.Commands.DeleteBooking;
using RoomFlow.Application.Messaging;
using RoomFlow.Application.Tests.Fakes;
using RoomFlow.Domain.Entities;

namespace RoomFlow.Application.Tests.Features.Bookings.Commands;

public sealed class DeleteBookingCommandHandlerTests
{
    [Fact]
    public async Task Handle_returns_false_when_booking_is_missing()
    {
        var fixture = CreateFixture();

        var deleted = await fixture.Handler.Handle(
            new DeleteBookingCommand(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        Assert.False(deleted);
        Assert.Empty(fixture.Emails.Messages);
    }

    [Fact]
    public async Task Handle_throws_when_booking_belongs_to_another_user()
    {
        var fixture = CreateFixture();
        var booking = Seed(fixture.Bookings, fixture.User.Id);

        await Assert.ThrowsAsync<BookingNotOwnedException>(
            () => fixture.Handler.Handle(new DeleteBookingCommand(booking.Id, Guid.NewGuid()), CancellationToken.None));
        Assert.Single(fixture.Bookings.Bookings);
        Assert.Empty(fixture.Emails.Messages);
    }

    [Fact]
    public async Task Handle_removes_booking_when_caller_is_owner()
    {
        var fixture = CreateFixture();
        var booking = Seed(fixture.Bookings, fixture.User.Id);

        var deleted = await fixture.Handler.Handle(
            new DeleteBookingCommand(booking.Id, fixture.User.Id),
            CancellationToken.None);

        Assert.True(deleted);
        Assert.Empty(fixture.Bookings.Bookings);
    }

    [Fact]
    public async Task Handle_enqueues_cancelled_email_after_booking_is_removed()
    {
        var fixture = CreateFixture();
        var booking = Seed(fixture.Bookings, fixture.User.Id);

        await fixture.Handler.Handle(
            new DeleteBookingCommand(booking.Id, fixture.User.Id),
            CancellationToken.None);

        var message = Assert.Single(fixture.Emails.Messages);
        Assert.Equal(BookingEmailEventType.Cancelled, message.EventType);
        Assert.Equal(booking.Id, message.BookingId);
        Assert.Equal(fixture.User.Email, message.UserEmail);
        Assert.Equal(booking.UserDisplayName, message.UserDisplayName);
        Assert.Equal(booking.RoomName, message.RoomName);
        Assert.Equal(booking.StartsAt, message.StartsAt);
        Assert.Equal(booking.EndsAt, message.EndsAt);
    }

    [Fact]
    public async Task Handle_deletes_booking_when_email_queue_fails()
    {
        var fixture = CreateFixture();
        fixture.Emails.PublishException = new InvalidOperationException("Service Bus unavailable");
        var booking = Seed(fixture.Bookings, fixture.User.Id);

        var deleted = await fixture.Handler.Handle(
            new DeleteBookingCommand(booking.Id, fixture.User.Id),
            CancellationToken.None);

        Assert.True(deleted);
        Assert.Empty(fixture.Bookings.Bookings);
        Assert.Empty(fixture.Emails.Messages);
    }

    private static Fixture CreateFixture()
    {
        var store = new FakeBookingStore();
        var users = new FakeUserReadStore();
        var emails = new FakeBookingEmailQueue();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "ada@example.com",
            PasswordHash = "hash",
            FirstName = "Ada",
            LastName = "Lovelace",
            CreatedAt = DateTimeOffset.UtcNow
        };
        users.Users.Add(user);

        var handler = new DeleteBookingCommandHandler(
            store,
            store,
            users,
            emails,
            NullLogger<DeleteBookingCommandHandler>.Instance);

        return new Fixture(handler, store, emails, user);
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

    private sealed record Fixture(
        DeleteBookingCommandHandler Handler,
        FakeBookingStore Bookings,
        FakeBookingEmailQueue Emails,
        User User);
}
