using API.Models;
using System.Text.Json;
using Confluent.Kafka;
using static Confluent.Kafka.ConfigPropertyNames;

namespace KafkaWebApiDemo.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly string _topic;
        public KafkaConsumerService(IConfiguration config, ILogger<KafkaConsumerService> logger)
        {
            _config = config;
            _logger = logger;

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _config["Kafka:BootstrapServers"],
                GroupId = _config["Kafka:GroupId"],
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
            _topic = _config["Kafka:Topic"];
        }

        //protected override Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    _consumer.Subscribe(_topic);

        //    return Task.Run(() =>
        //    {
        //        try
        //        {
        //            while (!stoppingToken.IsCancellationRequested)
        //            {
        //                var result = _consumer.Consume(stoppingToken);

        //                _logger.LogInformation($"Consumed: {result.Message.Value}");
        //            }
        //        }
        //        catch (OperationCanceledException)
        //        {
        //            _consumer.Close();

        //        }
        //    });
        //}

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_topic);
            return Task.Run(() =>
            {

                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var consumeResult = _consumer.Consume(stoppingToken);
                        if (consumeResult.Message.Value is string)
                        {
                            _logger.LogInformation($"Consumed: {consumeResult.Message.Value}");
                        }
                        else
                        {


                            var booking = JsonSerializer.Deserialize<TicketBooking>(consumeResult.Message.Value);

                            _logger.LogInformation($"Processing booking: {booking.BookingId}");


                            var confirmation = new BookingConfirmation
                            {
                                BookingId = booking.BookingId,
                                EventId = booking.EventId,
                                UserId = booking.UserId,
                                Quantity = booking.Quantity,
                                Status = "Confirmed",
                                Message = "Booking confirmed successfully"
                            };

                            _logger.LogInformation($"Booking confirmed: {JsonSerializer.Serialize(confirmation)}");
                        }
                    }
                }
                
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing booking");
                    _consumer.Close();
                }
            });
        

         
        }
    }
}
