using Domain.Enums;

namespace API.Models
{
    public class BookingConfirmation
    {
        public int Id { get; set; }
        public Guid BookingId { get; set; }
        public string EventId { get; set; }
        public string UserId { get; set; }
        public int Quantity { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime ConfirmationTime { get; set; } = DateTime.Now;
        public string Message { get; set; }
      

        public TicketBooking  TicketBooking { get; set; }

    }
}
