using API.DTO;
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

     

        [HttpPost("CreateBooking")]
        public async Task<IActionResult> CreateBooking([FromBody] TicketBookingDTO booking)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
               var obj= await _producerService.ProduceBookingRequestAsync(booking);
                return Accepted(new {Id=obj.Id, Status = obj.Status});
            }
            catch (Exception ex)
            {

                return StatusCode(500, "Error processing your booking");
            }
        }
    }
}
