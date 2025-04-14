using System.ComponentModel.DataAnnotations;
using API.Models;
using Domain.Enums;

namespace API.DTO
{
    public class TicketDTO
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }
        public DateTime BookingTime { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime? ProcessedTime { get; set; }
    }
    public class TicketBookingDTO
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public int Quantity { get; set; }
       // public DateTime BookingTime { get; set; } = DateTime.UtcNow;
    }
    public static class MapClass
    {
        public static TicketBooking Map(this TicketBookingDTO dto)
        {
            return new TicketBooking()
            {
                Id = Guid.NewGuid(),
                 //EventId= "EventId",
                UserId = dto.UserId,
                Quantity = dto.Quantity,
                Status = BookingStatus.Pending,
                BookingTime = DateTime.Now,
                ProcessedTime =null
            };
        }
    }
}
