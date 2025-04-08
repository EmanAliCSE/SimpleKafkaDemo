using API.Models;
using System.Text.Json;
using Confluent.Kafka;
using static Confluent.Kafka.ConfigPropertyNames;

namespace KafkaWebApiDemo.Services
{
    public class KafkaProducerService : IDisposable
    {
        private readonly IConfiguration _config;
        private readonly string _topic;
        private readonly IProducer<Null, string> _producer;

        public KafkaProducerService(IConfiguration config)
        {
            _config = config;

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
                await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = message });
                Console.WriteLine($"Produced booking request: {message}");
            }
            catch (ProduceException<Null, string> ex)
            {
                Console.WriteLine($"Delivery failed: {ex.Error.Reason}");
            }
        }
        public void Dispose()
        {
            _producer.Flush(TimeSpan.FromSeconds(10));
            _producer.Dispose();
        }
    }
}
