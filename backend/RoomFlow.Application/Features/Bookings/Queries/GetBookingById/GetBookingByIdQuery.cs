using MediatR;
using RoomFlow.Application.Abstractions.Data;

namespace RoomFlow.Application.Features.Bookings.Queries.GetBookingById;

public record GetBookingByIdQuery(Guid Id) : IRequest<BookingDto?>;

public sealed class GetBookingByIdQueryHandler : IRequestHandler<GetBookingByIdQuery, BookingDto?>
{
    private readonly IBookingReadStore _readStore;

    public GetBookingByIdQueryHandler(IBookingReadStore readStore)
    {
        _readStore = readStore;
    }

    public Task<BookingDto?> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
        => _readStore.GetByIdAsync(request.Id, cancellationToken);
}
