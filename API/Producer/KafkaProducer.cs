using API.Models;
using System.Text.Json;
using Confluent.Kafka;
using static Confluent.Kafka.ConfigPropertyNames;
using Domain.Data;

namespace KafkaWebApiDemo.Services
{
    public class KafkaProducerService
        //: IDisposable
    {
        private readonly IConfiguration _config;
        private readonly string _topic;
        private readonly IProducer<Null, string> _producer;
        private readonly IServiceScopeFactory _scopeFactory;

        public KafkaProducerService(IConfiguration config , IServiceScopeFactory scopeFactory)
        {
            _config = config;
            _scopeFactory = scopeFactory;
            _topic = _config["Kafka:Topic"];
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = _config["Kafka:BootstrapServers"]
            };
            _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
        }

        public async Task<string> SendMessageAsync(string message)
        {
            var result = await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = message });

            return $"Sent to: {result.TopicPartitionOffset}";
        }

        public async Task ProduceBookingRequestAsync(TicketBooking booking)
        {
            try
            {

                var message = JsonSerializer.Serialize(booking);

                // Save booking to database
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();


                dbContext.Bookings.Add(booking);
                await dbContext.SaveChangesAsync();

                await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = message });
                Console.WriteLine($"Produced booking request: {message}");
            }
            catch (ProduceException<Null, string> ex)
            {
                Console.WriteLine($"Delivery failed: {ex.Error.Reason}");
            }
        }
        //public void dispose()
        //{
        //    _producer.flush(timespan.fromseconds(10));
        //    _producer.dispose();
        //}
    }
}
