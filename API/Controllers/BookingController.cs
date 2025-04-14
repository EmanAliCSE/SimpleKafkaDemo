using API.DTO;
using API.Models;
using Domain.Enums;
using Infrastructure.Interfaces;
using KafkaWebApiDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace KafkaWebApiDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly KafkaProducerService _producerService;
        private readonly IUnitOfWork _uow;

        public BookingController(KafkaProducerService producerService , IUnitOfWork uow)
        {
            _producerService = producerService;
            _uow = uow;
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

        [HttpGet]
        public async Task<IActionResult> GetAllBookings()
        {
            var list = await _uow.Repository<TicketBooking>().ListAsync();
            return Ok(list);
        }
    }
}
