
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using API.Models;
using System;
using Domain.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Domain.Enums;

namespace KafkaWebApiDemo.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly string _topic;
        private readonly IServiceProvider _serviceProvider;

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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_topic);

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
                        if (booking != null)
                        {
                            using (var scope = _serviceProvider.CreateScope())
                            {
                                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                                // Check if booking exists
                                var existingBooking = await dbContext.Bookings

                                    .FirstOrDefaultAsync(b => b.Id == booking.Id, stoppingToken);

                                if (existingBooking != null)
                                {
                                    _logger.LogWarning($"Duplicate booking detected: {booking.Id}");
                                    continue;
                                }


                                booking.Status = BookingStatus.Processing;
                                dbContext.Bookings.Add(booking);
                                await dbContext.SaveChangesAsync(stoppingToken);


                                _logger.LogInformation($"Booking confirmed: {booking.Id}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing booking");
                _consumer.Close();
            }
         
        }
    }
}
