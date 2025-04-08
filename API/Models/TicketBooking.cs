﻿namespace API.Models
{
    public class TicketBooking
    {
        public string BookingId { get; set; } = Guid.NewGuid().ToString();
        public string EventId { get; set; }
        public string UserId { get; set; }
        public int Quantity { get; set; }
        public DateTime BookingTime { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending";
    }
}