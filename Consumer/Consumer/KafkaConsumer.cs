
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
        private readonly IPollyService _polly;
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly IConsumer<Null, string> _consumer;
        private readonly string _topic;
        private readonly IServiceScopeFactory _scopeFactory;

        public KafkaConsumerService(IConfiguration config, IPollyService polly, IServiceScopeFactory scopeFactory, ILogger<KafkaConsumerService> logger)
        {
            _config = config;
            _polly = polly;
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
                        var consumeResult = await _polly.RetryAsync(() =>
                        Task.FromResult(_consumer.Consume(stoppingToken)), "Kafka Consumer");


                        if (consumeResult.IsPartitionEOF)
                        {
                            _logger.LogError("IsPartitionEOF", "No msgs");
                            return;
                        }
                        var booking = JsonSerializer.Deserialize<TicketBooking>(consumeResult.Message.Value);
                        if (booking != null)
                        {
                            using var scope = _scopeFactory.CreateScope();
                            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                            bool result = await _polly.RetryAsync(
                                            () => ProcessBookingAsync(uow, booking),
                                            context: $"ProcessBooking - Id: {booking.Id}");
                            if (result)
                            {

                                await _polly.RetryAsync(() =>
                                {
                                    _consumer.Commit(consumeResult);
                                    return Task.CompletedTask;
                                }, context: $"Kafka Consumer Commit - Id: {booking.Id}");

                                // for testing 
                                LogHeaderIngo(consumeResult, _logger);
                            }
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
                     .FirstOrDefaultAsync(a => a.Key == booking.Id.ToString() && a.Topic == _topic);
                //&& a.Status == OutBoxStatus.Send);
                if (outboxMsg == null || outboxMsg.Status == OutBoxStatus.Consumed)
                {
                    _logger.LogInformation("Booking {Id} already processed at {Time}", booking.Id, outboxMsg?.ProcessedAt);
                    return false;
                }
               else
                {
                    outboxMsg.Status = OutBoxStatus.Consumed;
                    outboxMsg.ProcessedAt = DateTime.Now;

                    // modfy booking obj 
                    booking.ProcessedTime = DateTime.Now;
                    booking.Status = BookingStatus.Processing;

                    uow.Repository<TicketBooking>().Update(booking);
                    uow.Repository<OutboxMessage>().Update(outboxMsg);
                    await uow.SaveChangesAsync();
                }
               
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
  
    
}

