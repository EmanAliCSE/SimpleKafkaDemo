using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Domain.Interfaces;
using Infrastructure.Services;
using KafkaWebApiDemo.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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



            services.AddHostedService<OutboxProcessorService>();
            return services;
        }
    }
}
