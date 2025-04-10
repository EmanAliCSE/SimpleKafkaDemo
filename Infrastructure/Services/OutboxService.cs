using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class OutboxService: IOutboxService
    {
        private readonly AppDbContext _context;

        public OutboxService(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddMessageAsync<T>(string type, T message)
        {
            var outboxMessage = new OutboxMessage
            {
                
                Type = type,
                Content = JsonSerializer.Serialize(message)
            };

            await _context.OutboxMessages.AddAsync(outboxMessage);
            await _context.SaveChangesAsync();
        }

        public async Task<List<OutboxMessage>> GetPendingMessagesAsync(int batchSize = 100)
        {
            return await _context.OutboxMessages
                .Where(m => m.ProcessedAt == null)
                .OrderBy(m => m.CreatedAt)
                .Take(batchSize)
                .ToListAsync();
        }

        public async Task MarkAsProcessedAsync(Guid messageId)
        {
            var message = await _context.OutboxMessages.FindAsync(messageId);
            if (message != null)
            {
                message.ProcessedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAsFailedAsync(Guid messageId, string error)
        {
            var message = await _context.OutboxMessages.FindAsync(messageId);
            if (message != null)
            {
                message.Error = error;
                await _context.SaveChangesAsync();
            }
        }
    }
}
