// Services/KafkaProducerService.cs
using API.Models;
using Confluent.Kafka;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

public class KafkaProducerService : IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxService _outboxService;
    private readonly ILogger<KafkaProducerService> _logger;
    private readonly string _bookingRequestTopic;

    public KafkaProducerService(
        IConfiguration configuration,IUnitOfWork unitOfWork,
        IOutboxService outboxService, 
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
       _unitOfWork = unitOfWork;
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

    public async Task ProduceBookingRequestAsync(TicketBooking booking)
    {
        if(booking==null)
        { throw new ArgumentNullException(nameof(booking)); }
        var outboxMessage = new OutboxMessage
        {
            Key = booking.Id.ToString(),
            Topic = _bookingRequestTopic,
            Type = nameof(TicketBooking),
            Status = Domain.Enums.OutBoxStatus.Pending,
            Content = JsonSerializer.Serialize(booking)
        };
        _unitOfWork.Repository<TicketBooking>().Add(booking);
        _unitOfWork.Repository<OutboxMessage>().Add(outboxMessage);
       
       await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation($"Added booking request to outbox: {booking.Id}");
    }

   
    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
        GC.SuppressFinalize(this);
    }
}