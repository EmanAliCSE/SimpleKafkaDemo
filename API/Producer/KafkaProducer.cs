using Confluent.Kafka;

namespace KafkaWebApiDemo.Services
{
    public class KafkaProducerService
    {
        private readonly IConfiguration _config;
        private readonly string _bootstrapServers;
        private readonly string _topic;

        public KafkaProducerService(IConfiguration config)
        {
            _config = config;
            _bootstrapServers = _config["Kafka:BootstrapServers"];
            _topic = _config["Kafka:Topic"];
        }

        public async Task<string> SendMessageAsync(string message)
        {
            var config = new ProducerConfig { BootstrapServers = _bootstrapServers };

            using var producer = new ProducerBuilder<Null, string>(config).Build();

            var result = await producer.ProduceAsync(_topic, new Message<Null, string> { Value = message });

            return $"Sent to: {result.TopicPartitionOffset}";
        }
    }
}
