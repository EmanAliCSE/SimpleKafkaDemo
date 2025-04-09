using Domain.Enums;

namespace API.Models
{
    public class TicketBooking
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string EventId { get; set; }
        public string UserId { get; set; }
        public int Quantity { get; set; }
        public DateTime BookingTime { get; set; } = DateTime.UtcNow;
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public DateTime? ProcessedTime { get; set; }
        public string? ConfirmationMessage { get; set; }

      //  public virtual BookingConfirmation? BookingConfirmation { get; set; }

    }
}