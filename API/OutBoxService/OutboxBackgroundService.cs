// Services/OutboxProcessorService.cs
using System.Reflection.Metadata;
using Confluent.Kafka;
using Domain.Enums;
using Domain.Models;
using Helper.Constants;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

public class OutboxProcessorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessorService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);
    private readonly string _topic;
    public OutboxProcessorService(
         IServiceScopeFactory scopeFactory, IConfiguration config, 
        ILogger<OutboxProcessorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _topic = config["Kafka:Topic"];
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var producer = scope.ServiceProvider.GetRequiredService<IProducer<Null, string>>();

                var messages = await uow.Repository<OutboxMessage>()
                    .FindByCondition(m => m.Status ==  Domain.Enums.OutBoxStatus.Pending && m.RetryCount < OutBoxConstants.MaxRetryCount)
                    .OrderBy(m => m.CreatedAt)
                     .Take(10)
                    .ToListAsync(stoppingToken);

                foreach (var message in messages)
                {
                    try
                    {
                        await producer.ProduceAsync(_topic,
                            new Message<Null, string> { Value = message.Content });
                      
                        message.Status = Domain.Enums.OutBoxStatus.Send;
                        message.LastAttemptAt = DateTime.Now;
                        HeadersProcess(message);
                     
                    }
                    catch (ProduceException<Null,string> ex)
                    {
                        message.Error = ex.Message;
                        message.Status = OutBoxStatus.Failed;
                        message.LastAttemptAt = DateTime.Now;
                        message.RetryCount++;
                        if (message.RetryCount >= OutBoxConstants.MaxRetryCount)
                        {
                            message.Status = OutBoxStatus.Failed;
                            message.Error = OutBoxConstants.MaxRetryMsg;
                            _logger.LogWarning($"Outbox message {message.Id} permanently failed after 5 retries.");
                        }
                        // for test retry 
                        _logger.LogWarning($"Retry #{message.RetryCount} for message {message.Id}");

                       
                        _logger.LogError(ex, "Error processing outbox message");
                    }
                    uow.Repository<OutboxMessage>().Update(message);
                }
                await uow.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in outbox processor");
            }

           // await Task.Delay(5000, stoppingToken);
        }
    }

    protected void HeadersProcess(OutboxMessage message)
    {
        var kafkaMessage = new Message<Null, string>
        {
            Value = message.Content,
            Headers = new Headers()
        };

        // Optional: Add headers if present
        if (message.Headers != null)
        {
            foreach (var kvp in message.Headers)
            {
                kafkaMessage.Headers.Add(kvp.Key, System.Text.Encoding.UTF8.GetBytes(kvp.Value));
            }
        }
    }
}