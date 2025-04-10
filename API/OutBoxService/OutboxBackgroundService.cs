// Services/OutboxProcessorService.cs
using Confluent.Kafka;
using Domain.Models;
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
                    .FindByCondition(m => m.Status ==  Domain.Enums.OutBoxStatus.Pending)
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
                       
                    }
                    catch (ProduceException<Null,string> ex)
                    {
                        message.Error = ex.Message;
                        message.Status = Domain.Enums.OutBoxStatus.Failed;
                        await uow.SaveChangesAsync(stoppingToken);
                        _logger.LogError(ex, "Error processing outbox message");
                    }
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
}