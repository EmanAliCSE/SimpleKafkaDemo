namespace API.Models
{
    public class BookingConfirmation
    {
        public int Id { get; set; }
        public string BookingId { get; set; }
        public string EventId { get; set; }
        public string UserId { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
        public DateTime ConfirmationTime { get; set; } = DateTime.UtcNow;
        public string Message { get; set; }
    }
}
