using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Application.Features.Bookings.Queries.GetBookingById;
using RoomFlow.Application.Tests.Fakes;

namespace RoomFlow.Application.Tests.Features.Bookings.Queries;

public sealed class GetBookingByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_returns_booking_when_found()
    {
        var store = new FakeBookingStore();
        var booking = new BookingDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Salle A",
            Guid.NewGuid(),
            "Ada Lovelace",
            new DateTimeOffset(2026, 10, 26, 8, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 26, 9, 0, 0, TimeSpan.Zero));
        store.Bookings.Add(booking);
        var handler = new GetBookingByIdQueryHandler(store);

        var result = await handler.Handle(new GetBookingByIdQuery(booking.Id), CancellationToken.None);

        Assert.Equal(booking, result);
    }

    [Fact]
    public async Task Handle_returns_null_when_missing()
    {
        var handler = new GetBookingByIdQueryHandler(new FakeBookingStore());

        var result = await handler.Handle(new GetBookingByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }
}
