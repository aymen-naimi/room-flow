using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;
using RoomFlow.Application.Features.Bookings.Commands.CreateBooking;

namespace RoomFlow.Application.Tests.Features.Bookings.Commands;

public sealed class CreateBookingCommandValidatorTests
{
    private static readonly Guid RoomId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid UserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly DateTimeOffset Now = new(2026, 10, 25, 0, 30, 0, TimeSpan.Zero);

    private readonly CreateBookingCommandValidator _validator = new(new FakeTimeProvider(Now));

    [Fact]
    public void Valid_command_has_no_errors()
    {
        var result = _validator.TestValidate(Valid());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void StartsAt_in_the_past_is_rejected()
    {
        var command = Valid() with
        {
            StartsAt = new DateTimeOffset(2026, 10, 24, 23, 59, 0, TimeSpan.Zero),
            EndsAt = new DateTimeOffset(2026, 10, 25, 1, 0, 0, TimeSpan.Zero)
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.StartsAt);
    }

    [Fact]
    public void Duration_below_15_minutes_is_rejected()
    {
        var command = Valid() with
        {
            StartsAt = Now,
            EndsAt = Now.AddMinutes(14)
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.EndsAt);
    }

    [Fact]
    public void Duration_of_12_hours_is_accepted()
    {
        var command = Valid() with
        {
            StartsAt = Now,
            EndsAt = Now.AddHours(12)
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Duration_above_12_hours_is_rejected()
    {
        var command = Valid() with
        {
            StartsAt = Now,
            EndsAt = Now.AddHours(12).AddMinutes(1)
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.EndsAt);
    }

    [Fact]
    public void EndsAt_must_be_after_StartsAt()
    {
        var command = Valid() with
        {
            StartsAt = Now.AddHours(2),
            EndsAt = Now.AddHours(1)
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.EndsAt);
    }

    [Fact]
    public void RoomId_must_not_be_empty()
    {
        var result = _validator.TestValidate(Valid() with { RoomId = Guid.Empty });

        result.ShouldHaveValidationErrorFor(c => c.RoomId);
    }

    private static CreateBookingCommand Valid()
        => new(RoomId, UserId, Now.AddHours(1), Now.AddHours(2));
}
