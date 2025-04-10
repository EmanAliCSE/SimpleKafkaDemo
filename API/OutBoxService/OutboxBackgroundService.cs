// Services/OutboxProcessorService.cs
using Confluent.Kafka;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class OutboxProcessorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessorService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);
    private readonly string _topic;
    public OutboxProcessorService(
        IServiceProvider serviceProvider, IConfiguration config, 
        ILogger<OutboxProcessorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _topic = config["Kafka:Topic"];
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var producer = scope.ServiceProvider.GetRequiredService<IProducer<Null, string>>();

                var messages = await dbContext.OutboxMessages
                    .Where(m => m.Status ==  Domain.Enums.OutBoxStatus.Pending)
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
                        await dbContext.SaveChangesAsync(stoppingToken);
                        _logger.LogError(ex, "Error processing outbox message");
                    }
                }
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in outbox processor");
            }

           // await Task.Delay(5000, stoppingToken);
        }
    }
}