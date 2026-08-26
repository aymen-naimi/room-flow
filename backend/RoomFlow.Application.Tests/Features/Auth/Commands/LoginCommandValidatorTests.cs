using FluentValidation.TestHelper;
using RoomFlow.Application.Features.Auth.Commands.Login;

namespace RoomFlow.Application.Tests.Features.Auth.Commands;

public sealed class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Valid_command_has_no_errors()
    {
        var result = _validator.TestValidate(new LoginCommand("ada@example.com", "password1"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    public void Email_must_be_a_valid_address(string email)
    {
        var result = _validator.TestValidate(new LoginCommand(email, "password1"));

        result.ShouldHaveValidationErrorFor(command => command.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Password_must_not_be_empty(string password)
    {
        var result = _validator.TestValidate(new LoginCommand("ada@example.com", password));

        result.ShouldHaveValidationErrorFor(command => command.Password);
    }
}
