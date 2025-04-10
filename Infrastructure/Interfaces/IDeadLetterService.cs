using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public interface IDeadLetterService
    {
        Task SaveAsync(string topic, string key, string messageType, string message, string? reason, string? payload, string? exceptionMessage = "");
    }
}
