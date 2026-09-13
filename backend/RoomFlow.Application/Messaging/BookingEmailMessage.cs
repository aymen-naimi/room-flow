using System.Text.Json;
using System.Text.Json.Serialization;

namespace RoomFlow.Application.Messaging;

public enum BookingEmailEventType
{
    Created,
    Cancelled
}

public sealed record BookingEmailMessage(
    BookingEmailEventType EventType,
    Guid BookingId,
    string UserEmail,
    string UserDisplayName,
    string RoomName,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt);

public static class BookingEmailJson
{
    public static JsonSerializerOptions Options { get; } = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
