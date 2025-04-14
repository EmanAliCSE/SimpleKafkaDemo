using API.Controllers.Base;
using API.DTO;
using API.Features.Queries;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KafkaWebApiDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : BaseApiController
    {
        private readonly KafkaProducerService _producerService;

        public BookingController(KafkaProducerService producerService )
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

        [HttpGet]
        public async Task<IActionResult> GetAllBookings()
        {
            return Ok(await Mediator.Send(new GetTickets.Query()));

            //var list = await _uow.Repository<TicketBooking>().ListAsync();
            //return Ok(list);
        }
    }
}
