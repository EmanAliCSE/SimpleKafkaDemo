using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Interfaces
{
    public interface IOutboxService
    {
        Task AddMessageAsync<T>(string type, T message);
        Task<List<OutboxMessage>> GetPendingMessagesAsync(int batchSize = 100);
        Task MarkAsProcessedAsync(Guid messageId);
    }
}
