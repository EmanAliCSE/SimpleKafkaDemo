using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Enums;

namespace Domain.Models
{
    public class OutboxMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Key { get; set; }
        public string Topic { get; set; }
        public OutBoxStatus Status { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ProcessedAt { get; set; }
        public string? Error { get; set; }
        public int RetryCount { get; set; } = 0;
        public DateTime? LastAttemptAt { get; set; }

        public string? HeadersJson { get; set; }

        // Helper property to deserialize/serialize header automatically
        [NotMapped]
        public Dictionary<string, string>? Headers
        {
            get => string.IsNullOrEmpty(HeadersJson)
                ? null
                : JsonSerializer.Deserialize<Dictionary<string, string>>(HeadersJson);
            set => HeadersJson = value == null ? null : JsonSerializer.Serialize(value);
        }

    }
}
