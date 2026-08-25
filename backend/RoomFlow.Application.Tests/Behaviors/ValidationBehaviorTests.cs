using FluentValidation;
using MediatR;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Application.Behaviors;
using RoomFlow.Application.Features.Rooms.Commands.CreateRoom;

namespace RoomFlow.Application.Tests.Behaviors;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_throws_when_command_is_invalid()
    {
        var behavior = new ValidationBehavior<CreateRoomCommand, RoomDto>([new CreateRoomCommandValidator()]);
        var nextCalled = false;

        Task<RoomDto> Next(CancellationToken _)
        {
            nextCalled = true;
            return Task.FromResult(new RoomDto(Guid.NewGuid(), "x", 1, "y", DateTimeOffset.UtcNow));
        }

        await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(
                new CreateRoomCommand("", 0, ""),
                Next,
                CancellationToken.None));

        Assert.False(nextCalled);
    }

    [Fact]
    public async Task Handle_calls_next_when_command_is_valid()
    {
        var behavior = new ValidationBehavior<CreateRoomCommand, RoomDto>([new CreateRoomCommandValidator()]);
        var expected = new RoomDto(Guid.NewGuid(), "Salle A", 8, "RDC", DateTimeOffset.UtcNow);

        var result = await behavior.Handle(
            new CreateRoomCommand("Salle A", 8, "RDC"),
            _ => Task.FromResult(expected),
            CancellationToken.None);

        Assert.Equal(expected, result);
    }
}
