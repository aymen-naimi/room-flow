using MediatR;
using RoomFlow.Application.Abstractions.Data;

namespace RoomFlow.Application.Features.Rooms.Queries.GetRoomById;

public record GetRoomByIdQuery(Guid Id) : IRequest<RoomDto?>;

public sealed class GetRoomByIdQueryHandler : IRequestHandler<GetRoomByIdQuery, RoomDto?>
{
    private readonly IRoomReadStore _readStore;

    public GetRoomByIdQueryHandler(IRoomReadStore readStore)
    {
        _readStore = readStore;
    }

    public Task<RoomDto?> Handle(GetRoomByIdQuery request, CancellationToken cancellationToken)
        => _readStore.GetRoomByIdAsync(request.Id, cancellationToken);
}
