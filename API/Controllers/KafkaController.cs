using API.Models;
using Domain.Enums;
using KafkaWebApiDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace KafkaWebApiDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KafkaController : ControllerBase
    {
        private readonly KafkaProducerService _producerService;

        public KafkaController(KafkaProducerService producerService)
        {
            _producerService = producerService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] string message)
        {
            var result = await _producerService.SendMessageAsync(message);
            return Ok(result);
        }

        [HttpPost("CreateBooking")]
        public async Task<IActionResult> CreateBooking([FromBody] TicketBooking booking)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _producerService.ProduceBookingRequestAsync(booking);
                return Accepted(new { booking.Id, Status = BookingStatus.Processing });
            }
            catch (Exception ex)
            {
              
                return StatusCode(500, "Error processing your booking");
            }
        }
    }
}
