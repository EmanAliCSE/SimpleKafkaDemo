using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Constants
{
    public static class OutBoxConstants
    {
        public const int MaxRetryCount = 5;
        public const string MaxRetryMsg = "Permanently failed after max retries ";
    }
    public static class HeaderConstants
    {
        public const string MessageType = "message-type";
        public const string CorrelationId = "correlation-id";
        public const string SourceService = "source-service";

    }
    public static class ProducerConstants
    { 
        public const string BookingSouceService = "BookingService";

    }
}
