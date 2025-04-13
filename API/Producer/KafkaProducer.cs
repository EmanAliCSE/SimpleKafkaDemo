using API.DTO;
using API.Models;
using Confluent.Kafka;
using Domain.Interfaces;
using Domain.Models;
using Helper.Constants;
using Infrastructure.Interfaces;
using Infrastructure.Services;
using System.Text.Json;

public class KafkaProducerService : IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPollyService _polly;
    private readonly IOutboxService _outboxService;
    private readonly IDeadLetterService _deadLetterService;
    private readonly ILogger<KafkaProducerService> _logger;
    private readonly string _bookingRequestTopic;

    public KafkaProducerService(
        IConfiguration configuration,IUnitOfWork unitOfWork, IPollyService polly,
        IOutboxService outboxService, IDeadLetterService deadLetterService,
        ILogger<KafkaProducerService> logger)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            EnableIdempotence = true,
            MessageSendMaxRetries = OutBoxConstants.MaxRetryCount,
            Acks = Acks.All
        };
        _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
       _unitOfWork = unitOfWork;
       _polly = polly;
        _outboxService = outboxService;
        _deadLetterService = deadLetterService;
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

    public async Task<TicketBooking> ProduceBookingRequestAsync(TicketBookingDTO bookingDTO)
    {
        OutboxMessage outboxMessage = null;
        string bookingId = "";
        try
        {

       
        if(bookingDTO == null)
        { throw new ArgumentNullException(nameof(bookingDTO)); }
        TicketBooking booking = bookingDTO.Map();
            bookingId = booking.Id.ToString();
           
             outboxMessage = new OutboxMessage
            {
                Key = booking.Id.ToString(),
                Topic = _bookingRequestTopic,
                Type = nameof(TicketBooking),
                Status = Domain.Enums.OutBoxStatus.Pending,
                Content = JsonSerializer.Serialize(booking),
                Headers = new Dictionary<string, string> {
                        { HeaderConstants.MessageType, nameof(TicketBooking) },
                        { HeaderConstants.CorrelationId, booking.Id.ToString() },
                        { HeaderConstants.SourceService, ProducerConstants.BookingSouceService}
            }
            };
            await _polly.RetryAsync(async () =>
            {
                _unitOfWork.Repository<TicketBooking>().Add(booking);
                _unitOfWork.Repository<OutboxMessage>().Add(outboxMessage);

                await _unitOfWork.SaveChangesAsync();
            }, context: "Producer Save");
            var actionId = booking.Id;
            _logger.LogInformation($"{booking.Id}");
            _logger.LogInformation("Added booking request to TicketBooking: booking Id: {bookingId}", booking.Id);
            _logger.LogInformation("Added booking request to TicketBooking: booking Id: {BookingId}", booking.Id);

            //  _logger.LogInformation($"Added booking request to TicketBooking: {booking.Id}, ActionId: {actionId}", booking.Id, actionId);

            _logger.LogInformation("Added booking request to outbox: {bookingId}", bookingId);
        return booking;
        }
        catch (Exception ex)
        {
            if (outboxMessage!=null)
            {
                await _deadLetterService.SaveAsync(
                                   topic: _bookingRequestTopic,
                                   key: outboxMessage.Key,
                                   messageType: outboxMessage.Type,
                                   message: ex.Message,
                                   reason: "Kafka publish failure",
                                   payload: outboxMessage. Content,
                                   exceptionMessage: ex.ToString()
                               );
            }
            else
            {
                await _deadLetterService.SaveAsync(
                                          topic: _bookingRequestTopic,
                                          key: bookingId,
                                          messageType: nameof(TicketBooking),
                                          message: ex.Message,
                                          reason: "Kafka publish failure",
                                          payload: JsonSerializer.Serialize(bookingDTO),
                                          exceptionMessage: ex.ToString()
                                      );
            }
            _logger.LogError($"add to Dead Letter table : {ex.Message}", new { ActionId = bookingId });
                _logger.LogError($"Exception: {ex.Message} , {ex.ToString()}", new { ActionId = bookingId });
            throw ex;
        }
    }

   
    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
        GC.SuppressFinalize(this);
    }
}