using MediatR;
using RoomFlow.Application.Abstractions.Data;

namespace RoomFlow.Application.Features.Rooms.Queries.GetRooms;

public record GetRoomsQuery : IRequest<IReadOnlyList<RoomDto>>;

public sealed class GetRoomsQueryHandler : IRequestHandler<GetRoomsQuery, IReadOnlyList<RoomDto>>
{
    private readonly IRoomReadStore _readStore;

    public GetRoomsQueryHandler(IRoomReadStore readStore)
    {
        _readStore = readStore;
    }

    public Task<IReadOnlyList<RoomDto>> Handle(GetRoomsQuery request, CancellationToken cancellationToken)
        => _readStore.GetRoomsAsync(cancellationToken);
}
