using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum BookingStatus
    {
        Pending,    // Booking created but payment not processed
        Confirmed,  // Payment successful, booking confirmed
        Cancelled,  // Booking was cancelled
        Failed,     // Payment failed
        Expired    // Booking expired before payment
    }
}
