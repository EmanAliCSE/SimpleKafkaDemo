
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
using Infrastructure.Services;
using System.Reflection.Metadata;
using Helper.Constants;

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
                    using var scope = _scopeFactory.CreateScope();
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
                                LogHeaderInfo(consumeResult, _logger);
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        try
                        {
                            var dead = scope.ServiceProvider.GetRequiredService<IDeadLetterService>();

                            var consumeResult = _consumer.Consume(stoppingToken); // Re-fetch if necessary (if out of scope)
                            var headers = consumeResult.Message.Headers;

                            var keyHeader = headers.FirstOrDefault(h => h.Key == HeaderConstants.CorrelationId);
                            var messageTypeHeader = headers.FirstOrDefault(h => h.Key == HeaderConstants.MessageType);

                            var key = keyHeader != null ? System.Text.Encoding.UTF8.GetString(keyHeader.GetValueBytes()) : Guid.NewGuid().ToString();
                            var messageType = messageTypeHeader != null ? System.Text.Encoding.UTF8.GetString(messageTypeHeader.GetValueBytes()) : "Unknown";


                            await dead.SaveAsync(
                                topic: _topic,
                                key: key,
                                messageType: messageType,
                                message: ex.Message,
                                reason: "Failed during message consumption or processing.",
                                payload: consumeResult.Message.Value,
                                exceptionMessage: ex.ToString()
                            );
                            _logger.LogError($"Write message to DeadLetter .because of {ex.Message} with exception {ex.ToString()}",key);

                        }
                        catch (Exception dlqEx)
                        {
                            _logger.LogError($"Failed to write message to DeadLetter queue. with exception {dlqEx}");
                        }

                        _logger.LogError($"Error processing message with exception {ex.ToString()}");
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
        private void LogHeaderInfo(ConsumeResult<Null, string> consumerResult, ILogger _logger)
        {
            var headers = consumerResult.Message.Headers;
            var messageType = headers.FirstOrDefault(h => h.Key == "message-type")?.GetValueBytes();
            var correlationId = headers.FirstOrDefault(h => h.Key == "correlation-id")?.GetValueBytes();

            _logger.LogInformation($"Received {System.Text.Encoding.UTF8.GetString(messageType)} with Correlation ID: {System.Text.Encoding.UTF8.GetString(correlationId)}",correlationId);
        }
    }
  
    
}

