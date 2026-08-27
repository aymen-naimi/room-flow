using FluentValidation.TestHelper;
using RoomFlow.Application.Features.Rooms.Commands.DeleteRoom;

namespace RoomFlow.Application.Tests.Features.Rooms.Commands;

public sealed class DeleteRoomCommandValidatorTests
{
    private readonly DeleteRoomCommandValidator _validator = new();

    [Fact]
    public void Valid_command_has_no_errors()
    {
        var result = _validator.TestValidate(new DeleteRoomCommand(Guid.NewGuid()));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Id_must_not_be_empty()
    {
        var result = _validator.TestValidate(new DeleteRoomCommand(Guid.Empty));

        result.ShouldHaveValidationErrorFor(command => command.Id);
    }
}
