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

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_topic);

            return Task.Run(() =>
            {
                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var result = _consumer.Consume(stoppingToken);

                        _logger.LogInformation($"Consumed: {result.Message.Value}");
                    }
                }
                catch (OperationCanceledException)
                {
                    _consumer.Close();

                }
            });
        }
    }
}
