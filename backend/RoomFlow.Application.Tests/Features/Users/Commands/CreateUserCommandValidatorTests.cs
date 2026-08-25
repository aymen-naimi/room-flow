using FluentValidation.TestHelper;
using RoomFlow.Application.Features.Users.Commands.CreateUser;

namespace RoomFlow.Application.Tests.Features.Users.Commands;

public sealed class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator = new();

    [Fact]
    public void Valid_command_has_no_errors()
    {
        var result = _validator.TestValidate(
            new CreateUserCommand("ada@example.com", "password1", "Ada", "Lovelace"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    public void Email_must_be_a_valid_address(string email)
    {
        var result = _validator.TestValidate(
            new CreateUserCommand(email, "password1", "Ada", "Lovelace"));

        result.ShouldHaveValidationErrorFor(command => command.Email);
    }

    [Fact]
    public void Email_must_not_exceed_256_characters()
    {
        var email = new string('a', 245) + "@example.com";
        var result = _validator.TestValidate(
            new CreateUserCommand(email, "password1", "Ada", "Lovelace"));

        result.ShouldHaveValidationErrorFor(command => command.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    public void Password_must_be_at_least_8_characters(string password)
    {
        var result = _validator.TestValidate(
            new CreateUserCommand("ada@example.com", password, "Ada", "Lovelace"));

        result.ShouldHaveValidationErrorFor(command => command.Password);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void First_name_must_not_be_empty(string firstName)
    {
        var result = _validator.TestValidate(
            new CreateUserCommand("ada@example.com", "password1", firstName, "Lovelace"));

        result.ShouldHaveValidationErrorFor(command => command.FirstName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Last_name_must_not_be_empty(string lastName)
    {
        var result = _validator.TestValidate(
            new CreateUserCommand("ada@example.com", "password1", "Ada", lastName));

        result.ShouldHaveValidationErrorFor(command => command.LastName);
    }
}
