using MediatR;
using Microsoft.Extensions.Logging;
using RoomFlow.Application.Abstractions.Data;

namespace RoomFlow.Application.Features.Rooms.Commands.DeleteRoom;

public record DeleteRoomCommand(Guid Id) : IRequest<bool>;

public sealed class DeleteRoomCommandHandler : IRequestHandler<DeleteRoomCommand, bool>
{
    private readonly IRoomWriteStore _writeStore;
    private readonly ILogger<DeleteRoomCommandHandler> _logger;

    public DeleteRoomCommandHandler(IRoomWriteStore writeStore, ILogger<DeleteRoomCommandHandler> logger)
    {
        _writeStore = writeStore;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
    {
        var deleted = await _writeStore.RemoveAsync(request.Id, cancellationToken);
        if (deleted)
        {
            _logger.LogInformation("Room deleted {RoomId}", request.Id);
        }
        else
        {
            _logger.LogWarning(
                "Room deletion skipped because room {RoomId} was not found",
                request.Id);
        }

        return deleted;
    }
}
