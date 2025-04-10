using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class DeadLetterMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Topic { get; set; }
        public string Key { get; set; }
        public string Message { get; set; }
        public string MessageType { get; set; }
        public string? Payload { get; set; }
        public string? Reason { get; set; }
        public string? ExceptionMessage { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

     
    }
}
