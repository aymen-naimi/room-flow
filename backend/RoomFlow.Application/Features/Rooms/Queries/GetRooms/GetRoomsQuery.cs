using MediatR;
using Microsoft.Extensions.Logging;
using RoomFlow.Application.Abstractions.Data;

namespace RoomFlow.Application.Features.Rooms.Queries.GetRooms;

public record GetRoomsQuery : IRequest<IReadOnlyList<RoomDto>>;

public sealed class GetRoomsQueryHandler : IRequestHandler<GetRoomsQuery, IReadOnlyList<RoomDto>>
{
    private readonly IRoomReadStore _readStore;
    private readonly ILogger<GetRoomsQueryHandler> _logger;

    public GetRoomsQueryHandler(IRoomReadStore readStore, ILogger<GetRoomsQueryHandler> logger)
    {
        _readStore = readStore;
        _logger = logger;
    }

    public async Task<IReadOnlyList<RoomDto>> Handle(GetRoomsQuery request, CancellationToken cancellationToken)
    {
        var rooms = await _readStore.GetRoomsAsync(cancellationToken);
        _logger.LogInformation(
            "Listed {RoomCount} rooms",
            rooms.Count);
        return rooms;
    }
}
