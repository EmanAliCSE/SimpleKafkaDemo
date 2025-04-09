// Services/KafkaProducerService.cs
using API.Models;
using Confluent.Kafka;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

public class KafkaProducerService : IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly OutboxService _outboxService;
    private readonly ILogger<KafkaProducerService> _logger;
    private readonly string _bookingRequestTopic;
    private readonly string _bookingConfirmationTopic;

    public KafkaProducerService(
        IConfiguration configuration,
        OutboxService outboxService,
        ILogger<KafkaProducerService> logger)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            EnableIdempotence = true,
            MessageSendMaxRetries = 3,
            Acks = Acks.All
        };
        _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
        _outboxService = outboxService;
        _logger = logger;
        _bookingRequestTopic = configuration["Kafka:Topic"];
    }

    // For direct publishing (if needed)
    public async Task ProduceAsync(string topic, string message)
    {
        try
        {
            await _producer.ProduceAsync(_bookingRequestTopic, new Message<Null, string>
            {
                Value = message,
                Headers = new Headers
                    {
                        { "message-type", System.Text.Encoding.UTF8.GetBytes("booking-request") }
                    }
            });
            _logger.LogDebug($"Produced message to {topic}: {message}");
        }
        catch (ProduceException<Null, string> ex)
        {
            _logger.LogError(ex, $"Delivery failed for topic {topic}: {ex.Error.Reason}");
            throw;
        }
    }

    // Outbox pattern methods
    public async Task ProduceBookingRequestAsync(TicketBooking booking)
    {
        await _outboxService.AddMessageAsync("BookingCreated", booking);
        _logger.LogInformation($"Added booking request to outbox: {booking.Id}");
    }

    //public async Task ProduceBookingConfirmationAsync(BookingConfirmation confirmation)
    //{
    //    await _outboxService.AddMessageAsync("BookingConfirmed", confirmation);
    //    _logger.LogInformation($"Added booking confirmation to outbox: {confirmation.BookingId}");
    //}

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
        GC.SuppressFinalize(this);
    }
}