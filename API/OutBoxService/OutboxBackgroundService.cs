// Services/OutboxProcessorService.cs
using Confluent.Kafka;
using Domain.Data;
using Microsoft.EntityFrameworkCore;

public class OutboxProcessorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessorService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);

    public OutboxProcessorService(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
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
                    .Where(m => m.ProcessedAt == null)
                    .OrderBy(m => m.CreatedAt)
                    .Take(100)
                    .ToListAsync(stoppingToken);

                foreach (var message in messages)
                {
                    try
                    {
                        await producer.ProduceAsync("ticket-booking-requests",
                            new Message<Null, string> { Value = message.Content });

                        message.ProcessedAt = DateTime.UtcNow;
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        message.Error = ex.Message;
                        await dbContext.SaveChangesAsync(stoppingToken);
                        _logger.LogError(ex, "Error processing outbox message");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in outbox processor");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}