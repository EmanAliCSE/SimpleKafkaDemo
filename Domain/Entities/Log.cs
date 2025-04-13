using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Log
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string MessageTemplate { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Level { get; set; }
        public Guid? ActionId { get; set; }
        public string? RequestId { get; set; }
        public string? Application { get; set; }
        public string? Exception { get; set; }
        public string Properties { get; set; } // Contains structured data like BookingId
    }
}
