using System.Text.Json;
using Azure;
using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RoomFlow.Application.Messaging;

namespace RoomFlow.Functions;

public sealed class SendBookingEmail
{
    private readonly EmailClient _emailClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SendBookingEmail> _logger;

    public SendBookingEmail(
        EmailClient emailClient,
        IConfiguration configuration,
        ILogger<SendBookingEmail> logger)
    {
        _emailClient = emailClient;
        _configuration = configuration;
        _logger = logger;
    }

    [Function(nameof(SendBookingEmail))]
    public async Task Run(
        [ServiceBusTrigger("%ServiceBusQueueName%", Connection = "ServiceBus")]
        ServiceBusReceivedMessage message,
        CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Deserialize<BookingEmailMessage>(message.Body, BookingEmailJson.Options);
        if (payload is null)
        {
            throw new InvalidOperationException("Booking email message body is empty.");
        }

        var sender = _configuration["Email:SenderAddress"]
            ?? throw new InvalidOperationException("Email:SenderAddress is missing.");

        var content = new EmailContent(BookingEmailComposer.Subject(payload))
        {
            Html = BookingEmailComposer.Html(payload),
            PlainText = BookingEmailComposer.PlainText(payload)
        };
        var email = new EmailMessage(sender, payload.UserEmail, content);

        await _emailClient.SendAsync(WaitUntil.Started, email, cancellationToken);
        _logger.LogInformation(
            "Sent {EventType} booking email {BookingId} to {Email}",
            payload.EventType,
            payload.BookingId,
            payload.UserEmail);
    }
}
