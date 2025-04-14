using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Application.DTO
{
    public class OutboxMessageDTO
    {
        public Guid Id { get; set; } 
        public string Key { get; set; }
        public string Topic { get; set; }
        public OutBoxStatus Status { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? Error { get; set; }
        public int RetryCount { get; set; } 
        public DateTime? LastAttemptAt { get; set; }

        public string? HeadersJson { get; set; }
    }
}
