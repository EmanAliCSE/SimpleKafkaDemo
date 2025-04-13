
using System;
using Confluent.Kafka;
using KafkaWebApiDemo.Services;
using Microsoft.Identity.Client.Extensions.Msal;
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

            // health check 
            services.AddHealthChecks()
                     .AddSqlServer(
                         connectionString: configuration.GetConnectionString("DefaultConnection"),
                         name: "sqlserver",
                         healthQuery: "SELECT 1",
                         failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
                         tags: new[] { "db", "sql" }
                     )
                     .AddKafka(
                         setup =>
                         {
                             setup.BootstrapServers = configuration["Kafka:BootstrapServers"];
                         },
                         name: "kafka",
                         failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
                         tags: new[] { "messaging", "kafka" }
                     );
            //Add HealthChecks UI and in-memory storage
                services.AddHealthChecksUI(options =>
                {
                    options.SetEvaluationTimeInSeconds(15); // Check health every 15 seconds
                    options.MaximumHistoryEntriesPerEndpoint(60);
                    options.AddHealthCheckEndpoint("Service Health", "/health"); // Map to endpoint
                })
                .AddInMemoryStorage();



            return services;
        }
    }
}
