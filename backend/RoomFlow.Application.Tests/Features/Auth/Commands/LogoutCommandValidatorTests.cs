using FluentValidation.TestHelper;
using RoomFlow.Application.Features.Auth.Commands.Logout;

namespace RoomFlow.Application.Tests.Features.Auth.Commands;

public sealed class LogoutCommandValidatorTests
{
    private readonly LogoutCommandValidator _validator = new();

    [Fact]
    public void Valid_command_has_no_errors()
    {
        var result = _validator.TestValidate(new LogoutCommand("refresh-token"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RefreshToken_must_not_be_empty(string refreshToken)
    {
        var result = _validator.TestValidate(new LogoutCommand(refreshToken));

        result.ShouldHaveValidationErrorFor(command => command.RefreshToken);
    }

    [Fact]
    public void RefreshToken_must_not_exceed_maximum_length()
    {
        var result = _validator.TestValidate(new LogoutCommand(new string('a', 201)));

        result.ShouldHaveValidationErrorFor(command => command.RefreshToken);
    }
}
