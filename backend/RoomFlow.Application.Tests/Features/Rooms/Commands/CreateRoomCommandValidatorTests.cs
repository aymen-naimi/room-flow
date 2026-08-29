using FluentValidation.TestHelper;
using RoomFlow.Application.Features.Rooms.Commands.CreateRoom;

namespace RoomFlow.Application.Tests.Features.Rooms.Commands;

public sealed class CreateRoomCommandValidatorTests
{
    private readonly CreateRoomCommandValidator _validator = new();
    private static readonly Guid CreatedByUserId = Guid.NewGuid();

    [Fact]
    public void Valid_command_has_no_errors()
    {
        var result = _validator.TestValidate(new CreateRoomCommand("Salle A", 10, "RDC", CreatedByUserId));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Name_must_not_be_empty(string name)
    {
        var result = _validator.TestValidate(new CreateRoomCommand(name, 10, "RDC", CreatedByUserId));

        result.ShouldHaveValidationErrorFor(command => command.Name);
    }

    [Fact]
    public void Name_must_not_exceed_200_characters()
    {
        var result = _validator.TestValidate(new CreateRoomCommand(new string('a', 201), 10, "RDC", CreatedByUserId));

        result.ShouldHaveValidationErrorFor(command => command.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1001)]
    public void Capacity_must_be_between_1_and_1000(int capacity)
    {
        var result = _validator.TestValidate(new CreateRoomCommand("Salle A", capacity, "RDC", CreatedByUserId));

        result.ShouldHaveValidationErrorFor(command => command.Capacity);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Location_must_not_be_empty(string location)
    {
        var result = _validator.TestValidate(new CreateRoomCommand("Salle A", 10, location, CreatedByUserId));

        result.ShouldHaveValidationErrorFor(command => command.Location);
    }

    [Fact]
    public void Location_must_not_exceed_200_characters()
    {
        var result = _validator.TestValidate(new CreateRoomCommand("Salle A", 10, new string('a', 201), CreatedByUserId));

        result.ShouldHaveValidationErrorFor(command => command.Location);
    }
}
