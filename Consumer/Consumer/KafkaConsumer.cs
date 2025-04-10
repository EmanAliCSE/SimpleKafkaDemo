
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using API.Models;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Domain.Enums;
using Domain.Models;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using System.Text;

namespace KafkaWebApiDemo.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly IConsumer<Null, string> _consumer;
        private readonly string _topic;
        private readonly IServiceScopeFactory _scopeFactory;

        public KafkaConsumerService(IConfiguration config, IServiceScopeFactory scopeFactory, ILogger<KafkaConsumerService> logger)
        {
            _config = config;
            _logger = logger;
            _scopeFactory = scopeFactory;
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _config["Kafka:BootstrapServers"],
                GroupId = _config["Kafka:GroupId"],
               
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };
            _consumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();
            _topic = _config["Kafka:Topic"];
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_topic);
            return Task.Run(async () =>
            {

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(stoppingToken);
                        if (consumeResult.IsPartitionEOF)
                        {
                            _logger.LogError("IsPartitionEOF", "No msgs");
                            return;
                        }
                        var booking = JsonSerializer.Deserialize<TicketBooking>(consumeResult.Message.Value);
                       
                        using var scope = _scopeFactory.CreateScope();
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        bool result = await ProcessBookingAsync(uow, booking);
                        if (result)
                        {
                            _consumer.Commit(consumeResult);
                            // for testing 
                            LogHeaderIngo(consumeResult,_logger);
                        }

                    }

                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message");
                    }
                }

                _consumer.Close();
            });

        }
        private async Task<bool> ProcessBookingAsync(IUnitOfWork uow, TicketBooking booking)
        {
             await uow.BeginTransactionAsync();

            try
            {
                // check obj in outbox 
                var outboxMsg = await uow.Repository<OutboxMessage>()
                     .FirstOrDefaultAsync(a => a.Key == booking.Id.ToString() && a.Topic == _topic && a.Status == OutBoxStatus.Send);

                if (outboxMsg != null)
                {
                    outboxMsg.Status = OutBoxStatus.Consumed;
                    outboxMsg.ProcessedAt = DateTime.Now;
                }
                await uow.SaveChangesAsync();
                await uow.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await uow.RollbackTransactionAsync();
                return false;
                throw;
            }
        }
        private void LogHeaderIngo(ConsumeResult<Null, string> consumerResult, ILogger _logger)
        {
            var headers = consumerResult.Message.Headers;
            var messageType = headers.FirstOrDefault(h => h.Key == "message-type")?.GetValueBytes();
            var correlationId = headers.FirstOrDefault(h => h.Key == "correlation-id")?.GetValueBytes();

            _logger.LogInformation($"Received {System.Text.Encoding.UTF8.GetString(messageType)} with Correlation ID: {System.Text.Encoding.UTF8.GetString(correlationId)}");
        }
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

    //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    //{
    //    _consumer.Subscribe(_topic);

    //    try
    //    {
    //        while (!stoppingToken.IsCancellationRequested)
    //        {
    //            var consumeResult = _consumer.Consume(stoppingToken);
    //            if (consumeResult.Message.Value is string)
    //            {
    //                _logger.LogInformation($"Consumed: {consumeResult.Message.Value}");
    //            }
    //            else
    //            {
    //                var booking = JsonSerializer.Deserialize<TicketBooking>(consumeResult.Message.Value);
    //                if (booking != null)
    //                {
    //                    using (var scope = _serviceProvider.CreateScope())
    //                    {
    //                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    //                        // Check if booking exists
    //                        var existingBooking = await dbContext.Bookings

    //                            .FirstOrDefaultAsync(b => b.Id == booking.Id, stoppingToken);

    //                        if (existingBooking != null)
    //                        {
    //                            _logger.LogWarning($"Duplicate booking detected: {booking.Id}");
    //                            continue;
    //                        }


    //                        booking.Status = BookingStatus.Processing;
    //                        dbContext.Bookings.Add(booking);
    //                        await dbContext.SaveChangesAsync(stoppingToken);


    //                        _logger.LogInformation($"Booking confirmed: {booking.Id}");
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error processing booking");
    //        _consumer.Close();
    //    }

    //}
}

