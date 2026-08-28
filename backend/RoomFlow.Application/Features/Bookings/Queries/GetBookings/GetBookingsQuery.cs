using System.Globalization;
using MediatR;
using RoomFlow.Application.Abstractions.Data;

namespace RoomFlow.Application.Features.Bookings.Queries.GetBookings;

public record GetBookingsQuery(string? From, string? To, Guid? RoomId = null, Guid? UserId = null)
    : IRequest<IReadOnlyList<BookingDto>>;

public sealed class GetBookingsQueryHandler : IRequestHandler<GetBookingsQuery, IReadOnlyList<BookingDto>>
{
    private readonly IBookingReadStore _readStore;

    public GetBookingsQueryHandler(IBookingReadStore readStore)
    {
        _readStore = readStore;
    }

    public Task<IReadOnlyList<BookingDto>> Handle(GetBookingsQuery request, CancellationToken cancellationToken)
    {
        var from = DateTimeOffset.Parse(request.From!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
            .ToUniversalTime();
        var to = DateTimeOffset.Parse(request.To!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
            .ToUniversalTime();

        return _readStore.GetOverlappingAsync(from, to, request.RoomId, request.UserId, cancellationToken);
    }
}
