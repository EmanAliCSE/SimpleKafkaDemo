
using Confluent.Kafka;
using KafkaWebApiDemo.Services;
namespace Infrastructure
{
    public static  class ServicesExtensions
    {
        public static IServiceCollection AddServiceExtensions(
           this IServiceCollection services,
           IConfiguration configuration)
        {
            // Add Kafka services
            services.AddSingleton<IProducer<Null, string>>(sp =>
                 new ProducerBuilder<Null, string>(new ProducerConfig
                 {
                     BootstrapServers = configuration["Kafka:BootstrapServers"]
                 }).Build());

            //builder.Services.AddScoped<IOutboxService, OutboxService>();

            services.AddScoped<KafkaProducerService>();

            services.AddHostedService<KafkaConsumerService>();
            services.AddHostedService<OutboxProcessorService>();

           
            return services;
        }
    }
}
