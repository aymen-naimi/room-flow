using FluentValidation.TestHelper;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Application.Features.Bookings.Queries.GetBookings;
using RoomFlow.Application.Tests.Fakes;

namespace RoomFlow.Application.Tests.Features.Bookings.Queries;

public sealed class GetBookingsQueryValidatorTests
{
    private readonly GetBookingsQueryValidator _validator = new();

    [Fact]
    public void Valid_iso_range_has_no_errors()
    {
        var result = _validator.TestValidate(
            new GetBookingsQuery("2026-10-26T08:00:00Z", "2026-10-27T08:00:00Z"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null, "2026-10-26T08:00:00Z")]
    [InlineData("", "2026-10-26T08:00:00Z")]
    [InlineData("not-a-date", "2026-10-26T08:00:00Z")]
    [InlineData("2026-10-25T10:00:00", "2026-10-26T08:00:00Z")]
    public void From_must_be_iso8601_with_offset(string? from, string to)
    {
        var result = _validator.TestValidate(new GetBookingsQuery(from, to));

        result.ShouldHaveValidationErrorFor(query => query.From);
    }

    [Theory]
    [InlineData("2026-10-26T08:00:00Z", null)]
    [InlineData("2026-10-26T08:00:00Z", "nope")]
    public void To_must_be_iso8601_with_offset(string from, string? to)
    {
        var result = _validator.TestValidate(new GetBookingsQuery(from, to));

        result.ShouldHaveValidationErrorFor(query => query.To);
    }

    [Fact]
    public void From_must_be_before_to()
    {
        var result = _validator.TestValidate(
            new GetBookingsQuery("2026-10-26T11:00:00Z", "2026-10-26T10:00:00Z"));

        result.ShouldHaveValidationErrorFor(query => query);
    }

    [Fact]
    public void Range_must_not_exceed_8_days()
    {
        var result = _validator.TestValidate(
            new GetBookingsQuery("2026-10-26T08:00:00Z", "2026-11-04T08:00:01Z"));

        result.ShouldHaveValidationErrorFor(query => query);
    }

    [Fact]
    public void RoomId_must_not_be_empty_when_provided()
    {
        var result = _validator.TestValidate(
            new GetBookingsQuery("2026-10-26T08:00:00Z", "2026-10-27T08:00:00Z", Guid.Empty));

        result.ShouldHaveValidationErrorFor(query => query.RoomId);
    }

    [Fact]
    public void Optional_roomId_is_allowed()
    {
        var result = _validator.TestValidate(
            new GetBookingsQuery(
                "2026-10-26T08:00:00Z",
                "2026-10-27T08:00:00Z",
                Guid.Parse("11111111-1111-1111-1111-111111111111")));

        result.ShouldNotHaveAnyValidationErrors();
    }
}

public sealed class GetBookingsQueryHandlerTests
{
    [Fact]
    public async Task Handle_excludes_booking_that_ends_at_from()
    {
        var store = new FakeBookingStore();
        var from = new DateTimeOffset(2026, 10, 26, 10, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 10, 26, 11, 0, 0, TimeSpan.Zero);
        store.Bookings.Add(Booking(
            new DateTimeOffset(2026, 10, 26, 9, 0, 0, TimeSpan.Zero),
            from));
        var included = Booking(from, to);
        store.Bookings.Add(included);
        var handler = new GetBookingsQueryHandler(store);

        var result = await handler.Handle(
            new GetBookingsQuery("2026-10-26T10:00:00Z", "2026-10-26T11:00:00Z"),
            CancellationToken.None);

        Assert.Equal(included.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task Handle_filters_by_roomId()
    {
        var store = new FakeBookingStore();
        var from = new DateTimeOffset(2026, 10, 26, 10, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 10, 26, 11, 0, 0, TimeSpan.Zero);
        var roomId = Guid.NewGuid();
        var matching = Booking(from, to, roomId: roomId);
        store.Bookings.Add(matching);
        store.Bookings.Add(Booking(from, to, roomId: Guid.NewGuid()));
        var handler = new GetBookingsQueryHandler(store);

        var result = await handler.Handle(
            new GetBookingsQuery("2026-10-26T10:00:00Z", "2026-10-26T11:00:00Z", roomId),
            CancellationToken.None);

        Assert.Equal(matching.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task Handle_filters_by_userId()
    {
        var store = new FakeBookingStore();
        var from = new DateTimeOffset(2026, 10, 26, 10, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 10, 26, 11, 0, 0, TimeSpan.Zero);
        var userId = Guid.NewGuid();
        var matching = Booking(from, to, userId: userId);
        store.Bookings.Add(matching);
        store.Bookings.Add(Booking(from, to, userId: Guid.NewGuid()));
        var handler = new GetBookingsQueryHandler(store);

        var result = await handler.Handle(
            new GetBookingsQuery("2026-10-26T10:00:00Z", "2026-10-26T11:00:00Z", UserId: userId),
            CancellationToken.None);

        Assert.Equal(matching.Id, Assert.Single(result).Id);
    }

    private static BookingDto Booking(
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        Guid? roomId = null,
        Guid? userId = null)
        => new(
            Guid.NewGuid(),
            roomId ?? Guid.NewGuid(),
            "Salle A",
            userId ?? Guid.NewGuid(),
            "Ada Lovelace",
            startsAt,
            endsAt);
}
